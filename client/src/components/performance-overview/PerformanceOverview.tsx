import { AreaSeries, createChart, type UTCTimestamp } from 'lightweight-charts';
import './PerformanceOverview.css';
import { useEffect, useRef } from 'react';

const RangeSelector = () => {
    return (
        <div className="range-selector">
            <button className="range-button">1D</button>
            <button className="range-button">1W</button>
            <button className="range-button active">1M</button>
            <button className="range-button">3M</button>
            <button className="range-button">1Y</button>
        </div>
    );
};

const PerformanceOverview = () => {
    const chartContainerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!chartContainerRef.current) return;

        const chart = createChart(chartContainerRef.current, {
            width: chartContainerRef.current.clientWidth,
            height: 300,
        });
        const newSeries = chart.addSeries(AreaSeries, { lineColor: '#49cf7b', topColor: '#22c55e', bottomColor: '#e7f9ef' });
        const data = [{ time: 1642425322 as UTCTimestamp, value: 0 }, { time: 1642511722 as UTCTimestamp, value: 8 }, { time: 1642598122 as UTCTimestamp, value: 10 }, { time: 1642684522 as UTCTimestamp, value: 20 }, { time: 1642770922 as UTCTimestamp, value: 3 }, { time: 1642857322 as UTCTimestamp, value: 43 }, { time: 1642943722 as UTCTimestamp, value: 41 }, { time: 1643030122 as UTCTimestamp, value: 43 }, { time: 1643116522 as UTCTimestamp, value: 56 }, { time: 1643202922 as UTCTimestamp, value: 46 }, { time: 1643289322 as UTCTimestamp, value: 50 }, { time: 1643375722 as UTCTimestamp, value: 60 }, { time: 1643462122 as UTCTimestamp, value: 70 }, { time: 1643548522 as UTCTimestamp, value: 80 }, { time: 1643634922 as UTCTimestamp, value: 90 }, { time: 1643721322 as UTCTimestamp, value: 100 }];
        newSeries.setData(data);
        chart.timeScale().fitContent();

        return () => chart.remove();
    }, []);

    return (
        <div className="performance-overview">
            <div className='header flex justify-between items-center w-full mb-4'>
                <div className='header-content'>
                    <p className="heading">Performance Overview</p>
                    <span className='secondary-heading text-xs'>Nifty 50 Comparison</span>
                </div>
                <RangeSelector />
            </div>
            <div ref={chartContainerRef}>
            </div>
        </div>
    );
};

export default PerformanceOverview;