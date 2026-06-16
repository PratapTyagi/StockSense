/**
 * Function calculate total days from given date to now
 * @param { string } date Date
 * @returns string number of days
 */
export const totalDaysFromNow = (date: string) => {
    const givenDate = new Date(date);
    const now = new Date();
    // Calculate the difference in milliseconds
    const diffInMs = now - givenDate;
    // Convert milliseconds to days
    const diffInDays = diffInMs / (1000 * 60 * 60 * 24);

    return Math.floor(diffInDays);
}