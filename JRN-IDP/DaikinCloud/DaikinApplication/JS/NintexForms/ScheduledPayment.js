NWF.FormFiller.Events.RegisterAfterReady(function () {
    GetAttachment("Scheduled Payment", 271);
    //invokeSharePointService();
});

function GetCurrency() {
    try {
        let objek = { form_number: NWF$(".title input").val() };
        if (!objek.form_number) objek.form_number = NWF$("#" + title).val();
        NWF$.ajax({
            type: "POST",
            url: '/_layouts/15/WebServices/SchedulePaymentList.asmx/GetCurrencyBasedOnFormNumber',
            async: false,
            cache: false,
            data: JSON.stringify(objek),
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                const jsonData = JSON.parse(data.d);
                if (jsonData.Success) {
                    const currency = jsonData.Currency;
                    NWF$(".my-label label").text(currency);
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('Status : ', xhr.status, 'responseText: ', xhr.responseText, '-', thrownError);
            }
        });
    }
    catch (e) {
        console.log('GetCurrency : ' + e.message);
    }
};

function GetAttachment(ListName, ItemID) {
    try {
        const objek = { listName: ListName, listItemID: ItemID };
        $.ajax({
            type: "POST",
            url: 'https://sp3.daikin.co.id:8443/_vti_bin/lists.asmx',
            //url: '/layouts/15/daikin.application/webservices/ScheduledPayment.asmx/TestGetListAttachment',
            async: false,
            cache: false,
            data: JSON.stringify(objek),
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                const jsonData = JSON.parse(data.d);
                console.log(jsonData);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('Status : ', xhr.status, 'responseText: ', xhr.responseText, '-', thrownError);
            }
        });
    }
    catch (e) {
        console.log('GetAttachment : ' + e.message);
    }
};

function invokeSharePointService() {
    let xhr = new XMLHttpRequest();
    let url = "https://sp3.daikin.co.id:8443/_vti_bin/lists.asmx";  // SharePoint SOAP URL
    let soapRequest =
        `<?xml version="1.0" encoding="utf-8"?>
        <soap12:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap12="http://www.w3.org/2003/05/soap-envelope">
          <soap12:Body>
            <GetAttachmentCollection xmlns="http://schemas.microsoft.com/sharepoint/soap/">
              <listName>Scheduled Payment</listName>
              <listItemID>271</listItemID>
            </GetAttachmentCollection>
          </soap12:Body>
        </soap12:Envelope>
      `;

    xhr.open("POST", url, true);
    xhr.setRequestHeader("Content-Type", "text/xml");
    xhr.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/sharepoint/soap/GetAttachmentCollection");

    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            console.log("Response: ", xhr.responseText);
        } else if (xhr.readyState === 4) {
            console.error("Error: ", xhr.status, xhr.statusText);
        }
    };

    xhr.send(soapRequest);
}