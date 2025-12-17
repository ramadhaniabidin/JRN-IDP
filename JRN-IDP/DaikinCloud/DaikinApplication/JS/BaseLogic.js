function addCommas(nStr) {
    nStr += '';
    let x = nStr.split('.');
    let x1 = x[0];
    let x2 = x.length > 1 ? '.' + x[1] : '';
    let rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}

function GenerateGuid() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

function OnlyNumberAlsoDecimals(evt) {
    let charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode != 46 && charCode > 31
        && (charCode < 45 || charCode > 57)) {
        evt.preventDefault();
        return false;
    }
    return true;
}

function IsFileSizeExceeded(maxSizeInMB) {
    let isValid = false;
    for (let i = 0; i < NWF$('input[type=file]').length; i++) {
        if (NWF$('input[type=file]')[i].files.length > 0 && NWF$('input[type=file]')[i].files[0] !== null && NWF$('input[type=file]')[i].files[0] !== undefined) {
            if (NWF$('input[type=file]')[i].files[0].size > (maxSizeInMB * 1048600)) {
                // let sizeInMB = Math.round(NWF$('input[type=file]')[i].files[0].size / 10486) / 100; // # MB with two decimal places
                isValid = true;
                //alert("The maximum file size is " + maxSizeInMB + "MB, but the file " + NWF$('input[type=file')[i].files[0].name + " is " + sizeInMB + "MB. Please reduce the file size before uploading.");
                break;
            }
        }
    }
    return isValid;
}


function IsEmpty(str) {
    return (!str || 0 === str.length);
}

// this is the old method
//function GetQueryString() {
//    let vars = [], hash;
//    let hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
//    for (let i = 0; i < hashes.length; i++) {
//        hash = hashes[i].split('=');
//        vars.push(hash[0]);
//        vars[hash[0]] = hash[1];
//    }
//    console.log(vars);
//    return vars;
//}

function GetQueryString() {
    const params = {};
    const query = window.location.search.substring(1);

    if (!query) return params;

    query.split('&').forEach(pair => {
        const [key, value] = pair.split('=');
        params[key] = value;
    });
    console.log(params);
    return params;
}


function DateFormat_ddMMyyyy(date) {
    let monthNames = [
        "01", "02", "03",
        "04", "05", "06", "07",
        "08", "09", "10",
        "11", "12"
    ];

    let day = date.getDate();
    let monthIndex = date.getMonth();
    let year = date.getFullYear();
    // unused variables
    // let jam = date.getHours();
    // let menit = date.getMinutes();

    let formatDate = day + '-' + monthNames[monthIndex] + '-' + year.toString();

    return formatDate;
}

function DateFormat_ddMMyyyy2(date) {
    let monthNames = [
        "01", "02", "03",
        "04", "05", "06", "07",
        "08", "09", "10",
        "11", "12"
    ];

    let day = date.getDate();
    let monthIndex = date.getMonth();
    let year = date.getFullYear();
    // unused variables
    // let jam = date.getHours();
    // let menit = date.getMinutes();

    if (day < 10) {
        day += "0" + day;
    }

    let formatDate = day + '-' + monthNames[monthIndex] + '-' + year.toString();

    return formatDate;
}


function DateFormat_ddMMMyyyy(date) {
    let monthNames = [
        "Jan", "Feb", "Mar",
        "Apr", "May", "Jun", "Jul",
        "Aug", "Sep", "Oct",
        "Nov", "Dec"
    ];

    let day = date.getDate();
    let monthIndex = date.getMonth();
    let year = date.getFullYear();
    // unused variables
    // let jam = date.getHours();
    // let menit = date.getMinutes();

    let formatDate = day + '-' + monthNames[monthIndex] + '-' + year.toString();

    //let strDateTime = [formatDate, [AddZero(jam), AddZero(menit)].join(":"), jam >= 12 ? "PM" : "AM"].join(" ");
    //let strDateTime = [formatDate, [AddZero(jam), AddZero(menit)].join(":")].join(" ");

    return formatDate;
}

function JSONDateToJavaScript(jsonDateString) {
    let d = new Date(parseInt(jsonDateString.replace('/Date(', '')));
    let result = DateFormat_ddMMMyyyy(d);
    return result;
}

function GetWorkflowHistoryList() {
    try {
        const objek = {
            Form_No: NWF$("#" + cvFormNo).val(),
            Transaction_ID: NWF$("#" + cvTransID).val(),
            Module_Code: "M029"
        };
        NWF$.ajax({
            type: "POST",
            url: '/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetHistoryLog2',
            async: false,
            cache: false,
            data: JSON.stringify(objek),
            contentType: "application/json; charset=utf-8",
            success: onSuccessGetWorkflowHistoryLog,
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('Status : ', xhr.status, 'responseText: ', xhr.responseText, '-', thrownError);
            }
        });
    } catch (e) {
        console.log('GetWorkflowHistoryList : ' + e.message);
    }
};

function onSuccessGetWorkflowHistoryLog(response) {
    let trHTML = '';
    const parsedData = JSON.parse(response.d);
    const TotalRows = $("#tblHistory").find("tr:not(:first)").length;
    if (TotalRows > 0) {
        $("#tblHistory").find("tr:not(:first)").remove();
    }

    $.each(parsedData, function () {
        trHTML += '<tr><td>' + this.Personal_Name + '</td>';
        trHTML += '<td>' + this.Position + '</td>';
        trHTML += '<td>' + this.Comments + '</td>';
        trHTML += '<td>' + this.Action_Name + '</td>';
        trHTML += '<td>' + this.Action_Date + '</td></tr>';
    });
    $("#tblHistory > tbody").append(trHTML);
};