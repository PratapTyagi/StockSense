import { Switch } from '@headlessui/react';
import { totalDaysFromNow } from '../../helper/date';
import type { AlertStatus } from '../../types/AlertStatus';
import { Button } from '../../ui';
import AlertItem from '../alertItem/AlertItem';
import ImageTitleAndDescription from '../imageTitleAndDescription/ImageTitleAndDescription';
import './Alerts.css';
import { useEffect, useRef } from 'react';
import { AreaSeries, createChart, CrosshairMode, LineStyle } from 'lightweight-charts';

const totalAlerts = 12;
const totalTriggererdAlerts = 5;
const twoDaysAgo = () => {
    const date = new Date();
    date.setDate(date.getDate() - 2);
    return date;
};
const alerts = [
    {
        image: "/watch-list-item.png",
        title: "Reliance",
        description: "Price crosses above ₹3000.00",
        status: "Active" as AlertStatus
    },
    {
        image: "/watch-list-item.png",
        title: "TCS",
        description: "Volume spikes > 200% avg",
        status: "Paused" as AlertStatus,
        lastTriggered: twoDaysAgo()
    },
    {
        image: "/watch-list-item.png",
        title: "HDFCBANK",
        description: "% Down > 3% in 15min",
        status: "Triggered" as AlertStatus,
        lastTriggered: twoDaysAgo()
    }
];

const NotificationType = ({image, title, description, isNotificationEnabled, onToggle}: {image: string, title: string, description: string, isNotificationEnabled: boolean, onToggle: () => void}) => {
    return (
        <div className="notification-type flex items-center shadow-sm p-2">
            <ImageTitleAndDescription image={image} title={title} description={description} />
            <Switch
                checked={isNotificationEnabled}
                onChange={() => onToggle()}
                className="ml-auto group inline-flex h-6 w-11 items-center rounded-full bg-gray-200 transition data-checked:bg-green-400"
            >
                <span className="size-4 translate-x-1 rounded-full bg-white transition group-data-checked:translate-x-6" />
            </Switch>
        </div>
    )
}

const Alerts = () => {
    const chartContainerRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        const chart = createChart(chartContainerRef.current!, {
        height: 200,
        layout: {
            background: { color: "#ffffff" },
            textColor: "#6b7280",
        },
        grid: {
            vertLines: { visible: false },
            horzLines: { color: "#f0f0f0" },
        },
        rightPriceScale: {
            borderVisible: false,
        },
        timeScale: {
            borderVisible: false,
        },
        crosshair: {
            mode: CrosshairMode.Normal,
        },
        });

        // 🔵 Price Line (Blue)
        const areaSeries = chart.addSeries(AreaSeries, {
            lineColor: "#3b82f6",
            topColor: "rgba(59,130,246,0.25)",
            bottomColor: "rgba(59,130,246,0.05)",
            lineWidth: 2,
        });

        // 📊 Data (smooth curve like your UI)
        const priceData = [
            { time: "2026-01-01", value: 2920 },
            { time: "2026-01-02", value: 2915 },
            { time: "2026-01-03", value: 2925 },
            { time: "2026-01-04", value: 2935 },
            { time: "2026-01-05", value: 2928 },
            { time: "2026-01-06", value: 2932 },
            { time: "2026-01-07", value: 2942 },
            { time: "2026-01-08", value: 2945 },
            { time: "2026-01-09", value: 2940 },
            { time: "2026-01-10", value: 2943 },
            { time: "2026-01-11", value: 2950 },
            { time: "2026-01-12", value: 2948 },
        ];

        areaSeries.setData(priceData);

        chart.timeScale().fitContent();

        return () => chart.remove();
    }, []);

    return (
        <div className="alerts-container">
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

            <section className="alerts-content p-12 flex gap-4">
                <div className="alerts-list flex flex-col space-y-6 flex-1">
                    <div className="flex gap-4">
                        <div className="active-alerts shadow-sm flex-1 p-5">
                            <p className="secondary-heading">Active alerts</p>
                            <div className='info flex justify-between'>
                                <p className='total-alerts'>{totalAlerts}</p>
                                <span className='total-new-alerts'>3 new</span>
                            </div>
                        </div>
                        <div className="triggered-alerts shadow-sm flex-1 p-5">
                            <p className="secondary-heading">Triggered(24h)</p>
                            <div className='info flex justify-between'>
                                <p className='total-triggered-alerts'>{totalTriggererdAlerts}</p>
                                <span className='view-logs'>View logs</span>
                            </div>
                        </div>
                    </div>
                    <div className="your-alerts shadow-sm">
                        <p className="container-heading p-4 shadow-sm">Your Alerts</p>
                        <section className='content'>
                            <input type="text" placeholder='Search alerts...' className='m-3 shadow-sm'/>
                            <section className="list p-1">
                                {alerts.map(alert => {
                                    return <AlertItem image={alert.image} title={alert.title} description={alert.description} status={alert.status} lastTriggered={alert.lastTriggered ? String(totalDaysFromNow(alert.lastTriggered)): null}  />
                                })}
                            </section>
                        </section>
                    </div>
                </div>
                <div className='edit-alert shadow-sm p-4'>
                    <section className="header flex">
                        <div className='p-4'>
                            <p className="container-heading">Edit Alerts</p>
                            <span className="secondary-heading">Configure trigger conditions and notification channels</span>
                        </div>
                        <div className="actions flex gap-4 ml-auto p-4">
                            <Button label='Delete' onClick={() => {}} />
                            <Button label='Save Changes' isPrimary onClick={() => {}} />
                        </div>
                    </section>
                    <section className="conditions pl-4 pr-4 pt-4 pb-6">
                        <div className="left flex flex-col gap-5">
                            <div className="target-symbols">
                                <p className="secondary-heading mb-2">TARGET SYMBOL</p>
                                <div className="symbol shadow-sm flex items-center pt-2 pb-2 pl-4 pr-4">
                                    <ImageTitleAndDescription image='/watch-list-item.png' title='RELIANCE' description=''/>
                                    <span className="secondary-heading ml-auto">₹2,945.60</span>
                                </div>
                            </div>
                            <div className="condition flex flex-col gap-2">
                                <p className="secondary-heading mb-1">CONDITION</p>
                                <div className="flex gap-2">
                                    <Button style={{width: "50%"}} label='Price' onClick={() => {}}/>
                                    <Button style={{width: "50%"}} label='Volume' onClick={() => {}}/>
                                </div>
                                <Button label='Crosses Above' onClick={() => {}}/>
                            </div>
                            <div className="threshold-value">
                                <p className="secondary-heading mb-2">THRESHOLD VALUE</p>
                                <div className="input p-2 flex gap-2">
                                    <span>₹</span>
                                    <input type="text" placeholder='Trigger amount' />
                                </div>
                            </div>
                        </div>
                        <div className="right flex flex-col gap-4">
                            <div className="delivery-channels w-full">
                                <p className="secondary-heading mb-2">DELIVERY CHANNELS</p>
                                <div className="notification-types flex flex-col gap-2">
                                    <NotificationType image='/watch-list-item.png' title='In-app Notification' description='Shows in your dashboard inbox' isNotificationEnabled={true} onToggle={() => {}} />
                                    <NotificationType image='/watch-list-item.png' title='Email' description='rahul@gmail.com' isNotificationEnabled={true} onToggle={() => {}} />
                                </div>
                            </div>
                            <div className="frequency flex flex-col gap-2">
                                <p className="secondary-heading">FREQUENCY</p>
                                <input className="input p-2" type="text" disabled placeholder='Once per day' />
                                <span className='alert-message secondary-heading'>Alert will trigger a maximum of one time per trading day.</span>
                            </div>
                        </div>
                    </section>
                    <section className="price-context p-4">
                        <div className="heading flex">
                            <p className="container-heading">Price Context (1D)</p>
                            <span className='secondary-heading ml-auto'>Current: $2,945.60 | Target: $3,000.00</span>
                        </div>
                        <div className='mt-4' ref={chartContainerRef}>
                        </div>
                    </section>
                </div>
            </section>
        </div>
    )
}

export default Alerts