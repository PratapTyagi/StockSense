public static class Constants
{
    #region Domain names
    public const string ZerodhaKite = "https://kite.zerodha.com";
    public const string KiteAPI = "https://api.kite.trade";
    #endregion

    #region API endpoints
    public const string InstrumentsEndpoint = "/instruments";
    public const string HistoricalEndpoint = "/oms/instruments/historical";
    #endregion

    #region Cache keys

    #endregion

    #region Random constants

    public const string ZerodhaInstrumentsClient = "ZerodhaInstrumentsClient";
    public const string ZerodhaHistoricalClient = "ZerodhaHistoricalClient";
    #endregion


    #region Status codes
    public const int Success = 200;
    public const int BadRequest = 400;
    public const int InternalServerError = 500;
    #endregion
}