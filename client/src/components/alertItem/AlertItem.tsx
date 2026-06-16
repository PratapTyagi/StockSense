import type { AlertStatus } from "../../types/AlertStatus";
import ImageTitleAndDescription from "../imageTitleAndDescription/ImageTitleAndDescription";
import "./AlertItem.css"

const AlertItem = ({ image, title, description, status, lastTriggered }: { image: string, title: string; description: string, status: AlertStatus, lastTriggered: string | null }) => {
    const getStatusClassName = (status: AlertStatus): string => {
        switch (status) {
            case "Active":
                return "active";
            case "Triggered":
                return "triggered";
            case "Paused":
                return "paused";
            default:
                return "";
        }
    }
    return (
        <div className='alert-item m-3 shadow-sm p-2'>
            <div className="flex">
                <ImageTitleAndDescription image={image} title={title} description={description} />
                <span className={`status ml-auto ${getStatusClassName(status)}`}>{status}</span>
            </div>
            <span className="last-triggered secondary-heading flex justify-end mt-2">Last triggered: {lastTriggered || "Never"}</span>
        </div>
    )
}

export default AlertItem;