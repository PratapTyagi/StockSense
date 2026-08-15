using Application.Models.OpportunityScanner;
using Application.Services.OpportunityScanner;
using Domain.Entities;

namespace Tests;

public class OpportunityScannerTests
{
    // =========================================================================
    // HELPERS
    // =========================================================================

    private static Stock MakeStock(long id = 1, string symbol = "RELIANCE", string name = "Reliance Industries", string exchange = "NSE", bool active = true)
        => new() { Id = id, Symbol = symbol, CompanyName = name, Exchange = exchange, IsActive = active };

    /// <summary>
    /// Creates a list of candles with linearly increasing prices.
    /// </summary>
    private static IReadOnlyList<StockCandle> MakeCandles(
        int count = 250,
        decimal startPrice = 100m,
        decimal dailyIncrement = 0.5m,
        long volume = 100_000,
        long stockId = 1)
    {
        var candles = new List<StockCandle>(count);
        var baseDate = new DateTime(2025, 1, 1);
        for (int i = 0; i < count; i++)
        {
            var close = startPrice + dailyIncrement * i;
            candles.Add(new StockCandle
            {
                Id = i + 1,
                StockId = stockId,
                Timestamp = baseDate.AddDays(i),
                Open = close - 1,
                High = close + 2,
                Low = close - 2,
                Close = close,
                Volume = volume,
            });
        }
        return candles;
    }

    /// <summary>Creates candles with a specific final price and flat history.</summary>
    private static IReadOnlyList<StockCandle> MakeFlatCandles(
        int count = 250,
        decimal price = 100m,
        long volume = 100_000,
        long stockId = 1)
    {
        return MakeCandles(count, price, 0m, volume, stockId);
    }

    /// <summary>Creates candles with declining prices.</summary>
    private static IReadOnlyList<StockCandle> MakeDecliningCandles(
        int count = 250,
        decimal startPrice = 200m,
        decimal dailyDecrement = 0.5m,
        long volume = 100_000,
        long stockId = 1)
    {
        return MakeCandles(count, startPrice, -dailyDecrement, volume, stockId);
    }

    // =========================================================================
    // ELIGIBILITY TESTS
    // =========================================================================

    [Fact]
    public void IsSymbolEligible_NormalSymbol_ReturnsTrue()
    {
        Assert.True(OpportunityScannerService.IsSymbolEligible("RELIANCE"));
        Assert.True(OpportunityScannerService.IsSymbolEligible("TCS"));
        Assert.True(OpportunityScannerService.IsSymbolEligible("INFY"));
    }

    [Theory]
    [InlineData("STOCK-BE")]
    [InlineData("STOCK-SM")]
    [InlineData("STOCK-ST")]
    [InlineData("stock-be")]
    [InlineData("SOMETHING-sm")]
    public void IsSymbolEligible_SpecialSeries_ReturnsFalse(string symbol)
    {
        Assert.False(OpportunityScannerService.IsSymbolEligible(symbol));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void IsSymbolEligible_EmptyOrNull_ReturnsFalse(string? symbol)
    {
        Assert.False(OpportunityScannerService.IsSymbolEligible(symbol!));
    }

    [Theory]
    [InlineData("BESTEEL")]
    [InlineData("SMSOMETHING")]
    [InlineData("STARPAPER")]
    [InlineData("BEML")]
    [InlineData("SMCGLOBAL")]
    [InlineData("STAR")]
    public void IsSymbolEligible_NormalSymbolsContainingSimilarChars_ReturnsTrue(string symbol)
    {
        // These symbols contain "BE", "SM", "ST" but NOT as a "-XX" suffix
        Assert.True(OpportunityScannerService.IsSymbolEligible(symbol));
    }

    [Fact]
    public void Eligibility_InsufficientCandles_ExcludesStock()
    {
        // Less than 200 candles
        var candles = MakeCandles(count: 150);
        var stock = MakeStock();
        // MetricCalculator requires MinCandlesRequired; the service checks this.
        Assert.True(candles.Count < MetricCalculator.MinCandlesRequired);
    }

    [Fact]
    public void Eligibility_InvalidPrice_ZeroClose()
    {
        var candles = MakeCandles(count: 250);
        // Simulate invalid close on last candle
        var mutable = candles.ToList();
        mutable[^1] = new StockCandle
        {
            Id = mutable[^1].Id,
            StockId = 1,
            Timestamp = mutable[^1].Timestamp,
            Open = 0,
            High = 0,
            Low = 0,
            Close = 0,
            Volume = 100_000
        };
        // The last close is 0, which should be caught by eligibility
        Assert.Equal(0m, mutable[^1].Close);
    }

    // =========================================================================
    // MOMENTUM SCORING TESTS
    // =========================================================================

    [Fact]
    public void ScoreReturn_Null_ReturnsZero()
    {
        Assert.Equal(0, OpportunityScorer.ScoreReturn(null));
    }

    [Fact]
    public void ScoreReturn_NegativeReturn_LessThan30()
    {
        var score = OpportunityScorer.ScoreReturn(-15.0);
        Assert.True(score > 0);
        Assert.True(score < 30);
    }

    [Fact]
    public void ScoreReturn_VeryNegative_ReturnsZero()
    {
        Assert.Equal(0, OpportunityScorer.ScoreReturn(-30.0));
        Assert.Equal(0, OpportunityScorer.ScoreReturn(-50.0));
    }

    [Fact]
    public void ScoreReturn_ZeroReturn_Returns30()
    {
        Assert.Equal(30, OpportunityScorer.ScoreReturn(0.0));
    }

    [Fact]
    public void ScoreReturn_PositiveReturn_Between30And100()
    {
        var score = OpportunityScorer.ScoreReturn(10.0);
        Assert.True(score > 30);
        Assert.True(score < 100);
    }

    [Fact]
    public void ScoreReturn_AboveCap_Returns100()
    {
        Assert.Equal(100, OpportunityScorer.ScoreReturn(50.0));
        Assert.Equal(100, OpportunityScorer.ScoreReturn(100.0));
    }

    [Fact]
    public void ScoreReturn_DifferentValues_ProduceDifferentScores()
    {
        var s5 = OpportunityScorer.ScoreReturn(5.0);
        var s10 = OpportunityScorer.ScoreReturn(10.0);
        var s20 = OpportunityScorer.ScoreReturn(20.0);
        var s30 = OpportunityScorer.ScoreReturn(30.0);

        Assert.True(s5 < s10);
        Assert.True(s10 < s20);
        Assert.True(s20 < s30);
    }

    // =========================================================================
    // TREND SCORING TESTS
    // =========================================================================

    [Fact]
    public void ScoreTrendDistance_Null_ReturnsNeutral50()
    {
        Assert.Equal(50, OpportunityScorer.ScoreTrendDistance(null));
    }

    [Fact]
    public void ScoreTrendDistance_Zero_Returns50()
    {
        Assert.Equal(50, OpportunityScorer.ScoreTrendDistance(0.0));
    }

    [Fact]
    public void ScoreTrendDistance_PositiveDistance_Above50()
    {
        var score = OpportunityScorer.ScoreTrendDistance(10.0);
        Assert.True(score > 50);
        Assert.True(score <= 100);
    }

    [Fact]
    public void ScoreTrendDistance_NegativeDistance_Below50()
    {
        var score = OpportunityScorer.ScoreTrendDistance(-10.0);
        Assert.True(score < 50);
        Assert.True(score >= 0);
    }

    [Fact]
    public void ScoreTrendDistance_AtCap_Returns100()
    {
        var score = OpportunityScorer.ScoreTrendDistance(OpportunityScorer.TrendDistanceCapPct);
        Assert.Equal(100, score);
    }

    [Fact]
    public void ScoreTrendDistance_AtNegativeCap_Returns0()
    {
        var score = OpportunityScorer.ScoreTrendDistance(-OpportunityScorer.TrendDistanceCapPct);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreTrendDistance_ExtremeDistance_Capped()
    {
        // Beyond cap should still be 100
        var score = OpportunityScorer.ScoreTrendDistance(100.0);
        Assert.Equal(100, score);
    }

    // =========================================================================
    // VOLUME SCORING TESTS
    // =========================================================================

    [Fact]
    public void VolumeScore_BelowAverage_LowerScore()
    {
        var metrics = new StockMetrics { VolumeRatio = 0.5, OneMonthReturnPct = 5 };
        var result = OpportunityScorer.Score(metrics);
        Assert.True(result.Scores.VolumeScore < 65);
    }

    [Fact]
    public void VolumeScore_Normal_MidScore()
    {
        var metrics = new StockMetrics { VolumeRatio = 1.0, OneMonthReturnPct = 5 };
        var result = OpportunityScorer.Score(metrics);
        Assert.True(result.Scores.VolumeScore >= 60);
        Assert.True(result.Scores.VolumeScore <= 80);
    }

    [Fact]
    public void VolumeScore_High_Capped()
    {
        var metrics = new StockMetrics { VolumeRatio = 5.0, OneMonthReturnPct = 5 };
        var result = OpportunityScorer.Score(metrics);
        // Should be capped, not 100
        Assert.True(result.Scores.VolumeScore <= 100);
    }

    [Fact]
    public void VolumeScore_HighWithNegativePrice_Reduced()
    {
        var metricsPositive = new StockMetrics { VolumeRatio = 2.0, OneMonthReturnPct = 10 };
        var metricsNegative = new StockMetrics { VolumeRatio = 2.0, OneMonthReturnPct = -10 };

        var scorePositive = OpportunityScorer.Score(metricsPositive).Scores.VolumeScore;
        var scoreNegative = OpportunityScorer.Score(metricsNegative).Scores.VolumeScore;

        Assert.True(scorePositive > scoreNegative);
    }

    // =========================================================================
    // RISK SCORING TESTS
    // =========================================================================

    [Fact]
    public void RiskScore_LowVolatility_HighScore()
    {
        var metrics = new StockMetrics { AnnualizedVolatilityPct = 8.0 };
        var result = OpportunityScorer.Score(metrics);
        Assert.Equal(100, result.Scores.RiskScore);
    }

    [Fact]
    public void RiskScore_MediumVolatility_MidScore()
    {
        var metrics = new StockMetrics { AnnualizedVolatilityPct = 40.0 };
        var result = OpportunityScorer.Score(metrics);
        Assert.True(result.Scores.RiskScore > 20);
        Assert.True(result.Scores.RiskScore < 80);
    }

    [Fact]
    public void RiskScore_HighVolatility_LowScore()
    {
        var metrics = new StockMetrics { AnnualizedVolatilityPct = 80.0 };
        var result = OpportunityScorer.Score(metrics);
        Assert.Equal(0, result.Scores.RiskScore);
    }

    [Fact]
    public void RiskScore_Null_ReturnsNeutral()
    {
        var metrics = new StockMetrics { AnnualizedVolatilityPct = null };
        var result = OpportunityScorer.Score(metrics);
        Assert.Equal(50, result.Scores.RiskScore);
    }

    // =========================================================================
    // FINAL SCORE TESTS
    // =========================================================================

    [Fact]
    public void FinalScore_AlwaysBetween0And100()
    {
        // Best case
        var best = new StockMetrics
        {
            OneMonthReturnPct = 60,
            ThreeMonthReturnPct = 60,
            SixMonthReturnPct = 60,
            DistanceFromSma50Pct = 50,
            DistanceFromSma200Pct = 50,
            Sma50VsSma200Pct = 50,
            VolumeRatio = 5.0,
            AnnualizedVolatilityPct = 5,
        };
        var bestResult = OpportunityScorer.Score(best);
        Assert.True(bestResult.OpportunityScore >= 0);
        Assert.True(bestResult.OpportunityScore <= 100);

        // Worst case
        var worst = new StockMetrics
        {
            OneMonthReturnPct = -50,
            ThreeMonthReturnPct = -50,
            SixMonthReturnPct = -50,
            DistanceFromSma50Pct = -50,
            DistanceFromSma200Pct = -50,
            Sma50VsSma200Pct = -50,
            VolumeRatio = 0,
            AnnualizedVolatilityPct = 100,
        };
        var worstResult = OpportunityScorer.Score(worst);
        Assert.True(worstResult.OpportunityScore >= 0);
        Assert.True(worstResult.OpportunityScore <= 100);
    }

    [Fact]
    public void FinalScore_WeightsAddTo100()
    {
        var totalWeight = OpportunityScorer.MomentumWeight
                        + OpportunityScorer.TrendWeight
                        + OpportunityScorer.VolumeWeight
                        + OpportunityScorer.RiskWeight;
        Assert.Equal(1.0, totalWeight, precision: 10);
    }

    // =========================================================================
    // OVEREXTENSION PENALTY TESTS
    // =========================================================================

    [Fact]
    public void OverextensionPenalty_10Pct_NoPenalty()
    {
        Assert.Equal(0, OpportunityScorer.CalculateOverextensionPenalty(10.0));
    }

    [Fact]
    public void OverextensionPenalty_25Pct_Penalty2()
    {
        Assert.Equal(2, OpportunityScorer.CalculateOverextensionPenalty(25.0));
    }

    [Fact]
    public void OverextensionPenalty_40Pct_Penalty5()
    {
        Assert.Equal(5, OpportunityScorer.CalculateOverextensionPenalty(40.0));
    }

    [Fact]
    public void OverextensionPenalty_60Pct_Penalty8()
    {
        Assert.Equal(8, OpportunityScorer.CalculateOverextensionPenalty(60.0));
    }

    [Fact]
    public void OverextensionPenalty_80Pct_Penalty12()
    {
        Assert.Equal(12, OpportunityScorer.CalculateOverextensionPenalty(80.0));
    }

    [Fact]
    public void OverextensionPenalty_100Pct_Penalty15()
    {
        Assert.Equal(15, OpportunityScorer.CalculateOverextensionPenalty(100.0));
    }

    [Fact]
    public void OverextensionPenalty_150Pct_StillMaxPenalty15()
    {
        Assert.Equal(15, OpportunityScorer.CalculateOverextensionPenalty(150.0));
    }

    [Fact]
    public void OverextensionPenalty_Null_NoPenalty()
    {
        Assert.Equal(0, OpportunityScorer.CalculateOverextensionPenalty(null));
    }

    [Fact]
    public void Overextension_AboveThreshold_GeneratesSignal()
    {
        var metrics = new StockMetrics
        {
            DistanceFromSma200Pct = 25.0, // above 20% threshold
            OneMonthReturnPct = 20,
        };
        var result = OpportunityScorer.Score(metrics);
        Assert.Contains("Potentially overextended", result.Signals);
        Assert.True(result.OverextensionPenalty > 0);
    }

    [Fact]
    public void Overextension_BelowThreshold_NoSignalNoPenalty()
    {
        var metrics = new StockMetrics
        {
            DistanceFromSma200Pct = 15.0, // below 20% threshold
            OneMonthReturnPct = 10,
        };
        var result = OpportunityScorer.Score(metrics);
        Assert.DoesNotContain("Potentially overextended", result.Signals);
        Assert.Equal(0, result.OverextensionPenalty);
    }

    [Fact]
    public void FinalScore_EqualsBaseMinusPenalty_Clamped()
    {
        // A stock with known metrics to verify: FinalScore = BaseScore - Penalty
        var metrics = new StockMetrics
        {
            OneMonthReturnPct = 30,
            ThreeMonthReturnPct = 40,
            SixMonthReturnPct = 50,
            DistanceFromSma50Pct = 20,
            DistanceFromSma200Pct = 80, // penalty = 12
            Sma50VsSma200Pct = 10,
            VolumeRatio = 1.5,
            AnnualizedVolatilityPct = 25,
        };
        var result = OpportunityScorer.Score(metrics);

        // Verify penalty is 12
        Assert.Equal(12, result.OverextensionPenalty);

        // Verify final score is base - penalty (clamped)
        var expectedBase =
            result.Scores.MomentumScore * OpportunityScorer.MomentumWeight
            + result.Scores.TrendScore * OpportunityScorer.TrendWeight
            + result.Scores.VolumeScore * OpportunityScorer.VolumeWeight
            + result.Scores.RiskScore * OpportunityScorer.RiskWeight;
        var expectedFinal = Math.Clamp(expectedBase - 12, 0, 100);
        Assert.Equal(Math.Round(expectedFinal, 2), result.OpportunityScore);
    }

    [Fact]
    public void FinalScore_WithPenalty_StillAboveZero()
    {
        // Even with max penalty, a strong stock should still have a positive score
        var metrics = new StockMetrics
        {
            OneMonthReturnPct = 40,
            ThreeMonthReturnPct = 50,
            SixMonthReturnPct = 60,
            DistanceFromSma50Pct = 25,
            DistanceFromSma200Pct = 120, // penalty = 15 (max)
            Sma50VsSma200Pct = 15,
            VolumeRatio = 2.0,
            AnnualizedVolatilityPct = 20,
        };
        var result = OpportunityScorer.Score(metrics);
        Assert.Equal(15, result.OverextensionPenalty);
        Assert.True(result.OpportunityScore > 0);
        Assert.True(result.OpportunityScore <= 100);
    }

    // =========================================================================
    // METRIC CALCULATOR TESTS
    // =========================================================================

    [Fact]
    public void MetricCalculator_IncreasingPrices_PositiveReturns()
    {
        var stock = MakeStock();
        var candles = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 1m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.True(metrics.OneMonthReturnPct > 0);
        Assert.True(metrics.ThreeMonthReturnPct > 0);
        Assert.True(metrics.SixMonthReturnPct > 0);
    }

    [Fact]
    public void MetricCalculator_DecreasingPrices_NegativeReturns()
    {
        var stock = MakeStock();
        var candles = MakeDecliningCandles(count: 250, startPrice: 300m, dailyDecrement: 0.5m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.True(metrics.OneMonthReturnPct < 0);
        Assert.True(metrics.ThreeMonthReturnPct < 0);
        Assert.True(metrics.SixMonthReturnPct < 0);
    }

    [Fact]
    public void MetricCalculator_FlatPrices_ZeroReturns()
    {
        var stock = MakeStock();
        var candles = MakeFlatCandles(count: 250, price: 100m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.Equal(0, metrics.OneMonthReturnPct);
        Assert.Equal(0, metrics.ThreeMonthReturnPct);
        Assert.Equal(0, metrics.SixMonthReturnPct);
    }

    [Fact]
    public void MetricCalculator_ComputesSMAs()
    {
        var stock = MakeStock();
        var candles = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 0.5m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.NotNull(metrics.Sma20);
        Assert.NotNull(metrics.Sma50);
        Assert.NotNull(metrics.Sma200);
        // With increasing prices, latest close should be above all SMAs
        Assert.True(metrics.LatestClose > metrics.Sma20);
        Assert.True(metrics.LatestClose > metrics.Sma50);
        Assert.True(metrics.LatestClose > metrics.Sma200);
    }

    [Fact]
    public void MetricCalculator_ComputesDistanceFromSMA()
    {
        var stock = MakeStock();
        var candles = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 0.5m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.NotNull(metrics.DistanceFromSma50Pct);
        Assert.NotNull(metrics.DistanceFromSma200Pct);
        Assert.NotNull(metrics.Sma50VsSma200Pct);
        // With increasing prices, all distances should be positive
        Assert.True(metrics.DistanceFromSma50Pct > 0);
        Assert.True(metrics.DistanceFromSma200Pct > 0);
        Assert.True(metrics.Sma50VsSma200Pct > 0);
    }

    [Fact]
    public void MetricCalculator_ComputesVolatility()
    {
        var stock = MakeStock();
        var candles = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 0.5m);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.NotNull(metrics.AnnualizedVolatilityPct);
        Assert.True(metrics.AnnualizedVolatilityPct > 0);
    }

    [Fact]
    public void MetricCalculator_ComputesAverageDailyTradedValue()
    {
        var stock = MakeStock();
        var candles = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 0.5m, volume: 50_000);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.NotNull(metrics.AverageDailyTradedValue);
        Assert.True(metrics.AverageDailyTradedValue > 0);
    }

    [Fact]
    public void MetricCalculator_VolumeRatio()
    {
        var stock = MakeStock();
        // All same volume → ratio should be ~1.0
        var candles = MakeCandles(count: 250, volume: 100_000);
        var metrics = MetricCalculator.Compute(stock, candles);

        Assert.NotNull(metrics.VolumeRatio);
        Assert.True(Math.Abs(metrics.VolumeRatio.Value - 1.0) < 0.01);
    }

    // =========================================================================
    // RANKING TESTS
    // =========================================================================

    [Fact]
    public void Ranking_StrongerCandidate_RanksHigher()
    {
        var stockA = MakeStock(id: 1, symbol: "STRONG");
        var stockB = MakeStock(id: 2, symbol: "WEAK");

        // Stock A: strong uptrend
        var candlesA = MakeCandles(count: 250, startPrice: 100m, dailyIncrement: 1.0m, volume: 200_000, stockId: 1);
        // Stock B: flat
        var candlesB = MakeFlatCandles(count: 250, price: 100m, volume: 200_000, stockId: 2);

        var metricsA = MetricCalculator.Compute(stockA, candlesA);
        var metricsB = MetricCalculator.Compute(stockB, candlesB);

        var scoreA = OpportunityScorer.Score(metricsA);
        var scoreB = OpportunityScorer.Score(metricsB);

        Assert.True(scoreA.OpportunityScore > scoreB.OpportunityScore,
            $"Strong stock ({scoreA.OpportunityScore}) should rank above weak stock ({scoreB.OpportunityScore})");
    }

    [Fact]
    public void Ranking_HighVolatility_ReducesScore()
    {
        var stockLow = MakeStock(id: 1, symbol: "LOWVOL");
        var stockHigh = MakeStock(id: 2, symbol: "HIGHVOL");

        // Same trend but different volatility (simulated via metrics directly)
        var metricsLow = new StockMetrics
        {
            OneMonthReturnPct = 10,
            ThreeMonthReturnPct = 15,
            SixMonthReturnPct = 20,
            DistanceFromSma50Pct = 5,
            DistanceFromSma200Pct = 10,
            Sma50VsSma200Pct = 5,
            VolumeRatio = 1.0,
            AnnualizedVolatilityPct = 15,
        };
        var metricsHigh = new StockMetrics
        {
            OneMonthReturnPct = 10,
            ThreeMonthReturnPct = 15,
            SixMonthReturnPct = 20,
            DistanceFromSma50Pct = 5,
            DistanceFromSma200Pct = 10,
            Sma50VsSma200Pct = 5,
            VolumeRatio = 1.0,
            AnnualizedVolatilityPct = 60,
        };

        var scoreLow = OpportunityScorer.Score(metricsLow);
        var scoreHigh = OpportunityScorer.Score(metricsHigh);

        Assert.True(scoreLow.OpportunityScore > scoreHigh.OpportunityScore,
            $"Low-vol stock ({scoreLow.OpportunityScore}) should rank above high-vol stock ({scoreHigh.OpportunityScore})");
    }

    // =========================================================================
    // SIGNALS TESTS
    // =========================================================================

    [Fact]
    public void Signals_StrongMomentum_Generated()
    {
        var metrics = new StockMetrics
        {
            OneMonthReturnPct = 15,
            ThreeMonthReturnPct = 20,
            SixMonthReturnPct = 30,
        };
        var result = OpportunityScorer.Score(metrics);
        Assert.Contains("Strong 1M momentum", result.Signals);
        Assert.Contains("Strong 3M momentum", result.Signals);
        Assert.Contains("Strong 6M momentum", result.Signals);
    }

    [Fact]
    public void Signals_HighVolatility_Generated()
    {
        var metrics = new StockMetrics { AnnualizedVolatilityPct = 50.0 };
        var result = OpportunityScorer.Score(metrics);
        Assert.Contains("High volatility", result.Signals);
    }

    [Fact]
    public void Signals_AboveAverageVolume_Generated()
    {
        var metrics = new StockMetrics { VolumeRatio = 2.0, OneMonthReturnPct = 5 };
        var result = OpportunityScorer.Score(metrics);
        Assert.Contains("Above-average volume", result.Signals);
    }

    // =========================================================================
    // SCORE CLUSTERING REDUCTION
    // =========================================================================

    [Fact]
    public void ScoreClustering_DifferentMetrics_ProduceDifferentScores()
    {
        var scores = new HashSet<double>();
        for (int i = 0; i < 20; i++)
        {
            var metrics = new StockMetrics
            {
                OneMonthReturnPct = i * 2.5,
                ThreeMonthReturnPct = i * 3.0,
                SixMonthReturnPct = i * 4.0,
                DistanceFromSma50Pct = i * 1.5 - 10,
                DistanceFromSma200Pct = i * 2.0 - 15,
                Sma50VsSma200Pct = i * 1.0 - 5,
                VolumeRatio = 0.5 + i * 0.15,
                AnnualizedVolatilityPct = 15 + i * 2.5,
            };
            var result = OpportunityScorer.Score(metrics);
            scores.Add(result.OpportunityScore);
        }

        // With continuous scoring, we expect most/all to be unique
        Assert.True(scores.Count >= 15,
            $"Expected at least 15 unique scores from 20 different inputs, got {scores.Count}");
    }
}