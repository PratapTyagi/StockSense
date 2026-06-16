const ImageTitleAndDescription = ({ image, title, description }: { image: string; title: string; description: string }) => {
    return (
        <div className="flex items-center space-x-4">
            <img src={image} alt={title} className="w-8 h-8 rounded-full" />
            <div className="flex flex-col">
                <p className="font-medium">{title}</p>
                <span className="text-xs secondary-heading ">{description}</span>
            </div>
        </div>
    )
}

export default ImageTitleAndDescription;