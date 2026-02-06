setInterval(function () {
    dateshow();
},1000);
function dateshow() {
    
    var date = new Date();
    var day = date.getDate();
    var month = date.getMonth() + 1;
    var year = date.getFullYear();
    const monthshort = date.toLocaleString('default', { month: 'long' });
    document.getElementById("dtshow").innerHTML = day + " " + monthshort + " " + year;

}