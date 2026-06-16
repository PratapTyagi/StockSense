import { useEffect, useRef } from "react";
import "./Compare.css";
import { ColorType, createYieldCurveChart, LineSeries, type DeepPartial, type YieldCurveChartOptions } from "lightweight-charts";
import ComparisonTable from "../../ui/comparisonTable/ComparisonTable";
import ImageTitleAndDescription from "../imageTitleAndDescription/ImageTitleAndDescription";

const NormalizedPerformance = () => {
    const chartContainerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!chartContainerRef.current) return;

        const chartOptions: DeepPartial<YieldCurveChartOptions> = {
            layout: { textColor: 'black', background: { type: ColorType.Solid, color: 'white' } },
            yieldCurve: { baseResolution: 1, minimumTimeRange: 10, startTimeRange: 3 },
            handleScroll: false, handleScale: false,
            height: 300,
            width: chartContainerRef.current.clientWidth
        };

        const chart = createYieldCurveChart(chartContainerRef.current, chartOptions);
        const lineSeries = chart.addSeries(LineSeries, { color: '#2962FF' });
        const lineSeries2 = chart.addSeries(LineSeries, { color: 'red' });
        const lineSeries3 = chart.addSeries(LineSeries, { color: 'purple' });

        lineSeries.setData([{ time: 0, value: 0 },{ time: 1, value: 1 }, { time: 3, value: -1 }, { time: 4, value: 4 }, { time: 5, value: 7 }, { time: 6, value: 12 }, { time: 7, value: 10 }, { time: 8, value: 15 }, { time: 9, value: 14 }, { time: 10, value: 17 }, { time: 11, value: 15 }, { time: 12, value: 17 }]);
        lineSeries2.setData([{ time: 0, value: 0 },{ time: 1, value: -2 },{ time: 3, value: -5 },{ time: 4, value: 1 },{ time: 5, value: 3 },{ time: 6, value: 5 },{ time: 7, value: 4 },{ time: 8, value: 8 },{ time: 9, value: 7 },{ time: 10, value: 10 },{ time: 11, value: 11 },{ time: 12, value: 12 }]);
        lineSeries3.setData([{ time: 0, value: 0 },{ time: 1, value: 1 },{ time: 3, value: 3 },{ time: 4, value: 2 },{ time: 5, value: -1 },{ time: 6, value: -3 },{ time: 7, value: -2 },{ time: 8, value: -5 },{ time: 9, value: -4 },{ time:10, value: -6 },{ time: 11, value: -3 },{ time: 12, value: -4 }]);

        chart.timeScale().fitContent();

        return () => chart.remove();
    }, []);

    return (
        <div className="normalized-performance">
            <div className="header pb-8">
                <p className="container-heading">Normalized Performance</p>
            </div>
            <div className="performance-stats" ref={chartContainerRef}>
            </div>

            <div className="chart-description">
                <p className="secondary-heading"><span className="color-blue"></span> Stock A  <span className="color-red">Red Line</span> - Stock B  <span className="color-purple">Purple Line</span> - Stock C</p>
            </div>
        </div>
    )
}



const MetricComparison = () => {
    const stocks = [
        { name: <ImageTitleAndDescription image="/temp-profile-pic.png" title="RELIANCE" description="₹2945.60" />, yearChange: 1.2, marketCap: '19.8T', peRatio: 28.4, eps: 103.4, revenueGrowthYOY: "14.2%" },
        { name: <ImageTitleAndDescription image="/temp-profile-pic.png" title="TCS" description="₹3850.75" />, yearChange: -0.8, marketCap: '14.2T', peRatio: 32.1, eps: 120.2, revenueGrowthYOY: "8.2%" },
        { name: <ImageTitleAndDescription image="/temp-profile-pic.png" title="HDFCBANK" description="₹1642.10" />, yearChange: -0.8, marketCap: '12.5T', peRatio: 16.8, eps: 84.5, revenueGrowthYOY: "5.1%" },
    ];

    const rows = [
        {
            label: '1Y % Change',
            accessor: 'yearChange',
            render: (item: any) => (
            <span className={item.yearChange >= 0 ? 'color-green' : 'color-red'}>
                {item.yearChange}%
            </span>
            ),
        },
        {
            label: 'Market Cap',
            accessor: 'marketCap',
        },
        {
            label: 'P/E Ratio',
            accessor: 'peRatio',
        },
        {
            label: 'EPS',
            accessor: 'eps',
        },
        {
            label: 'Revenue Growth (YoY)',
            accessor: 'revenueGrowthYOY',
        },
    ];

    
    return (
        <div className="metric-comparison">
            <section className="header flex w-full items-center justify-between p-6">
                <p className="container-heading">Metric Comparison</p>
                <div className="flex space-x-2">
                    <span className="metric active">Fundamentals</span>
                    <span className="metric">Technicals</span>
                </div>
            </section>
            <ComparisonTable items={stocks} rows={rows} />
        </div>
    )
}

const Compare = () => {
    return (
        <div className="compare-container">
            <section
                className="dashboard-navbar"
                style={{
                width: "85vw",
                background: "white",
                padding: "20px",
                borderLeft: "1px solid grey",
                }}
            >
                Navbar
            </section>

            <div className="compare-content">
                <NormalizedPerformance />
                <MetricComparison />
            </div>
        </div>
    )
}

export default Compare