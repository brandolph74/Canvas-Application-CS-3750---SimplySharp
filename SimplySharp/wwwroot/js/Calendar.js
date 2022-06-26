let eventsArr = [];

let eventsTable = document.getElementById("eventsTable");

let trElements = eventsTable.getElementsByTagName("tr");
for (let tr of trElements) {
    let tdElements = tr.getElementsByTagName("td");
    let eventObj = {
        id: tdElements[0].innerText,
        title: tdElements[0].innerText + " " + tdElements[1].innerText,
        visStart: tdElements[2].innerText,
        visEnd: tdElements[3].innerText,
        start: tdElements[2].innerText,
        end: tdElements[3].innerText,
        allDay: false,
        color: tdElements[4].innerText
    };
    eventsArr.push(eventObj);
}

//Assignment Events
let assgnEventsArr = [];

let assgnEventsTable = document.getElementById("assgnEventsTable");

let assgnTrElements = assgnEventsTable.getElementsByTagName("tr");
for (let tr of assgnTrElements) {
    let assgnTdElements = tr.getElementsByTagName("td");
    let assgnEventObj = {
        id: assgnTdElements[0].innerText,
        title: assgnTdElements[1].innerText + " " + assgnTdElements[2].innerText,
        visStart: assgnTdElements[4].innerText,
        visEnd: assgnTdElements[3].innerText,
        start: assgnTdElements[4].innerText,
        end: assgnTdElements[3].innerText,
        allDay: false,
        url: '../Assignments/Details/' + assgnTdElements[0].innerText // Sets url when event is clicked. Goes to assignment details page.
    };
    assgnEventsArr.push(assgnEventObj);
}


let allEvents = [];
allEvents[0] = eventsArr;
allEvents[1] = assgnEventsArr;

let calendarEl = document.getElementById("calendar");

let calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "timeGridWeek",
    headerToolbar: {
        left: "prev,next today",
        center: "title",
        right: "dayGridMonth,timeGridWeek,timeGridDay",
    },
    displayEventTime: false,
});


//Source: https://stackoverflow.com/questions/23872898/set-events-in-fullcalendar-from-array
//This allows for the adding of two different types of events.
calendar.batchRendering(() => {
    //remove all events
    calendar.getEvents().forEach(event => event.remove());

    //add all events
    eventsArr.forEach(event => calendar.addEvent(event));
    assgnEventsArr.forEach(event => calendar.addEvent(event));

});

eventsTable.style.display = "none"
assgnEventsTable.style.display = "none"

calendar.render();