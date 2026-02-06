var objTimer;

setInterval(function () {

    DisplayDate();
}, 1000);


function DisplayDate() {
   
   
    var objDate = new Date;
    var intDate = objDate.getDate();
    var intDay = objDate.getDay();
    var intMonth = objDate.getMonth()+1;
    var intYear = objDate.getFullYear();
    var intHours = objDate.getHours();
    var intMinutes = objDate.getMinutes();
    var intSeconds = objDate.getSeconds()
    var strTimeValue = "" + intHours

    strTimeValue += ((intMinutes < 10) ? ":0" : ":") + intMinutes
    strTimeValue += ((intSeconds < 10) ? ":0" : ":") + intSeconds

    var strDate =  strTimeValue;
   // document.getElementById('clock').innerHTML = strDate;
    document.getElementById('clock').innerHTML = strDate;
   
    
}
/*intMonth + " /" + intDate + ", " + intYear + " " +*/