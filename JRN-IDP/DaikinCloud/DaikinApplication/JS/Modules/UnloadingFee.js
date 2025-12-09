

var popUpDialog;
var page_CurrIdx = 1;
var page_Count = 1;
var dataResult = [];
var dataResult_Details = [];
var inputSelectors = [];
var transNumber = '';
var selectedItem = [];
var sumTotalValue = 0;
var vendorName;

var options = [];
var tblHeaders = [];
var tblName = "";
var moduleName = "";
var trgtID = "";
var trgtCol = [];
var page_count = 0;
var detailTable = "";
var ColumnToFilter = "";
var FilterValue = "";
var targetID_Detail = [];
var targetCol_Detail = [];

//var isAvail_momentJs = false;
//try {
//    if (moment) {
//        isAvail_momentJs = true;
//    }
//}
//catch (error) {
//    console.log("BuktiTerimaBarang.js | " + error.name + ", " + error.message);
//    console.log(error.stack);
//}

NWF.FormFiller.Events.RegisterAfterReady(function () {

    currRow.find(".popupfilled").addClass("disableForm");
    //var currRow = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')").last();
    var currRow = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    currRow.find(".description").removeClass("hidden");
    currRow.find(".description label").html(('UNLOADING FEE - ') + vendorName.toUpperCase());

});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    var currRow = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    currRow.find(".dDescription").removeClass("hidden");
    var currRow1 = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')").first().find('.description label').html();
    currRow.find(".description label").html(('UNLOADING FEE - ') + vendorName.toUpperCase());
    currRow.find(".dDescription label").html(('UNLOADING FEE - ') + vendorName.toUpperCase());
    currRow.find(".dDescription input").val(('UNLOADING FEE - ') + vendorName.toUpperCase());
    currRow.find(".description input").val(('UNLOADING FEE - ') + vendorName.toUpperCase());

});

function PopUp_ShowDialog(shownPopup, module, currentRow) {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 450,
        width: 850,
        title: "Select : " + module
    });

    if (module == "UnloadingFee") {
        tblName = "dbo.MasterVendorUnloadingFee"
        moduleName = module;
        options = [
            { value: "Vendor_Name", text: "Vendor Name" },
            { value: "Title", text: "Vendor Number" },
            { value: "Bank_Account", text: "Bank Account Number" },
            { value: "Account_Holder", text: "Bank Account Name" },
            { value: "Payment_ID", text: "Payment ID" },
        ];
        tblHeaders = [
            { name: "Vendor Name", db_col: "Vendor_Name" },
            { name: "Vendor Number", db_col: "Title" },
            { name: "Payment ID", db_col: "Payment_ID" },
            { name: "Bank Account Number", db_col: "Bank_Account" },
            { name: "Bank Account Name", db_col: "Account_Holder" },
        ];

        trgtID = [VendorName, VendorNumber, AccountNumber, AccountName, PaymentID, BankKeyID];
        trgtCol = ["Vendor_Name", "Title", "Bank_Account", "Account_Holder", "Payment_ID", "Bank_Key"];

    }

    console.log("Pop up Module: " + module);
    $('#PopUp_Dropdown').val('');
    $('#PopUp_Keyword').val('');

    if (currentRow != null) {
        itemsRS_currSelected = currentRow;
    }

    $('#PopUp_Dropdown').html('');
    $.each(options, function (i, option) {
        $('#PopUp_Dropdown').append($('<option>', {
            text: option.text,
            value: option.value
        }));
    });

    $('#PopUp_TblHeader').html('');
    $.each(tblHeaders, function (i, header) {
        $('#PopUp_TblHeader').append($('<th>', {
            scope: "col",
            style: "text-align: center; color: white;",
            text: header.name,
            colspan: 3
        }));
    });
    $('#PopUp_TblHeader').append($('<th>', {
        scope: "col",
        style: "text-align: center; color: white;",
        text: "",
        colspan: 2
    }));

    PopUp_Search();
};

function PopUp_Search() {
    page_CurrIdx = 1;
    PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
}

function PopUp_Prev() {
    if (page_CurrIdx > 1) {
        page_CurrIdx = page_CurrIdx - 1;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
}

function PopUp_Next() {
    if (page_CurrIdx < page_count) {
        page_CurrIdx++;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
}

function PopUp_List(PageIndex, SearchBy, Keywords, SelectedItem) {
    try {
        console.log('Selected item : ', SelectedItem);
        $('#PopUp_TableBody').html('');
        if ((Keywords != undefined) || (Keywords != '')) {
            Keywords = $('#PopUp_Keyword').val();
        }
        else {
            Keywords = '';
        }

        if ((SearchBy != undefined) || (SearchBy != '')) {
            SearchBy = $('#PopUp_Dropdown').val();
        }
        else {
            SearchBy = '';
        }

        if (moduleName == "Supplier") {
            SearchBy += ";business_partner_category_name";
            Keywords += ";Supplier";
        }

        else if (moduleName == "Product") {
            SearchBy += ";[site]";
            Keywords += ";" + companyAbbr;
        }
    }
    catch (err) {
        console.log("PopUp Error: PopUp_List() \n" + err);
    }

    var param = {
        input: {
            searchTabl: tblName,
            searchCol: SearchBy,
            searchVal: Keywords,
            searchLike: 1,
            pageIndx: PageIndex,
            pageSize: 10,
        },

        output: {
            RecordCount: 0,
        }
    };
    console.log('Parameters: ', param);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
        data: JSON.stringify(param),
        dataType: "json",
        async: true,
        success: function (data) {
            const tBodyHTML = document.getElementById("PopUp_TableBody");
            //while (tBodyHTML.firstChild) {
            //    tBodyHTML.removeChild(tBodyHTML.lastChild);
            //}
            $('#PopUp_TableBody').html('');

            var jsonData = JSON.parse(data.d);
            console.log('JSON data : ', jsonData);

            dataResult = jsonData.Logs;

            var i = 0;
            $.each(jsonData.Logs, function (key, values) {
                var dataRow = document.createElement("tr");
                var dataCol = document.createElement("td");

                $.each(tblHeaders, function (HeaderID, data) {
                    dataCol = document.createElement("td");
                    dataCol.setAttribute("colspan", 3);
                    dataCol.setAttribute("style", "text-align:center");

                    var value = values.filter(e => e.Key == data.db_col)[0].Value;
                    if (typeof value === 'string' && value.indexOf("/Date(") >= 0) {
                        value = value.substring(6, 19);

                        //if (isAvail_momentJs) {
                        //    var formattedDate = moment(parseInt(value)).format("YYYY-MM-DD hh:mm:ss");
                        //    dataCol.innerHTML = formattedDate;
                        //} else if (isBase) {
                        //    var formattedDate = UNIXTimeStampeToSQLDate(parseInt(value));
                        //    dataCol.innerHTML = formattedDate;
                        //}
                    } else {
                        dataCol.innerHTML = value;
                    }

                    dataRow.appendChild(dataCol);
                    //console.log("data: " + data.db_col)
                });

                var colFour = document.createElement("td");
                colFour.setAttribute("colspan", 2);
                colFour.setAttribute("style", "text-align:center;cursor:pointer;color: blue;");
                colFour.innerHTML = "<a class=\"action-text\" onclick=\"PopUp_SelectItem(" + i + ")\">SELECT<\/a>";

                dataRow.appendChild(colFour);



                tBodyHTML.appendChild(dataRow);


                console.log('dataRow : ', tBodyHTML.rows[i]);

                i++;
            });
            console.log('tBodyHTML : ', tBodyHTML);
            if (parseInt(jsonData.TotalRecords) >= 0) {
                totalItemCnt = parseInt(jsonData.TotalRecords);
            }
            else {
                totalItemCnt = 0;
            }

            page_count = Math.ceil(totalItemCnt / param.input.pageSize);

            $('#info-paging_items').html('Page : ' + PageIndex.toString() + ' of ' + page_count.toString() + ' | ' + totalItemCnt.toString() + ' Results');
            i++
        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    });


}

function PopUp_SelectItem(id) {
    console.log(id + " module name: " + moduleName)
    if (moduleName == "Product") {
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {

            $.each(trgtID, function (index, values) {
                if (values == "product_name") {
                    itemsRS_currSelected.find("." + values + " textarea").val(dataResult[id].filter(e => e.Key == trgtCol[index])[0].Value);
                }
                else {
                    itemsRS_currSelected.find("." + values + " input").val(dataResult[id].filter(e => e.Key == trgtCol[index])[0].Value);
                }
                console.log(moduleName + " | set TargetID: " + values + " = " + dataResult[id].filter(e =>
                    e.Key == trgtCol[index]
                )[0].Value)

                itemsRS_currSelected.find("." + values + " input.nf-associated-control").prop('readonly', true);
            })
            itemsRS_currSelected = null;
        }
    }
    else if (moduleName == "UnloadingFee") {
        $.each(trgtCol, function (index, values) {
            var resultValue;
            if ((dataResult[id].filter(e => e.Key == values)[0] !== null) && (dataResult[id].filter(e => e.Key == values)[0] !== undefined)) {
                resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;
            }

            else if ((dataResult[id].filter(e => e.Key == values)[0] == null) || (dataResult[id].filter(e => e.Key == values)[0] == undefined)) {
                resultValue = "";
            }

            //var resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;

            if (typeof resultValue === 'string' && resultValue.startsWith('/Date(') && resultValue.endsWith(')/')) {
                var timestamp = parseInt(resultValue.match(/\/Date\((\d+)\)\//)[1], 10);
                var formattedDate = new Date(timestamp).toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
                NWF$('#' + trgtID[index]).val(formattedDate);
            } else {
                NWF$('#' + trgtID[index]).val(resultValue);
            }
        });

        var bankKeyID = NWF$("#" + BankKeyID).val();
        vendorName = NWF$("#" + VendorName).val();
        var param = {
            input: {
                searchTabl: "[dbo].[MasterBank]",
                searchCol: "code",
                searchVal: bankKeyID,
                searchLike: 1,
                pageIndx: 1,
                pageSize: 10,
            },

            output: {
                RecordCount: 0,
            }
        };

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function (data) {
                const tBodyHTML = document.getElementById("PopUp_TableBody");
                //while (tBodyHTML.firstChild) {
                //    tBodyHTML.removeChild(tBodyHTML.lastChild);
                //}
                $('#PopUp_TableBody').html('');

                var jsonData = JSON.parse(data.d);
                console.log('JSON data : ', jsonData);

                dataResult = jsonData.Logs[0];
                NWF$("#" + BankKeyName).val(dataResult.filter(x => x.Key == "Description")[0].Value);
                NWF$("#" + BankName).val(dataResult.filter(x => x.Key == "Description")[0].Value);
                //NWF$("#" + Desc).html(('Unloading Fee - ') + vendorName);

                //NWF$(".desc label").html(('Unloading Fee - ') + vendorName);
                //NWF$("#" + Desc).val(('Unloading Fee - ') + vendorName);

                var currRow = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
                currRow.find(".dDescription").removeClass("hidden");
                currRow.find(".description label").html(('UNLOADING FEE - ') + vendorName.toUpperCase());
                currRow.find(".dDescription label").html(('UNLOADING FEE - ') + vendorName.toUpperCase());
                currRow.find(".dDescription input").val(('UNLOADING FEE - ') + vendorName.toUpperCase());
                currRow.find(".description input").val(('UNLOADING FEE - ') + vendorName.toUpperCase());

            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });
    }

    else {
        $.each(trgtCol, function (index, values) {
            var resultValue;
            if ((dataResult[id].filter(e => e.Key == values)[0] !== null) && (dataResult[id].filter(e => e.Key == values)[0] !== undefined)) {
                resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;
            }

            else if ((dataResult[id].filter(e => e.Key == values)[0] == null) || (dataResult[id].filter(e => e.Key == values)[0] == undefined)) {
                resultValue = "";
            }

            //var resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;

            if (typeof resultValue === 'string' && resultValue.startsWith('/Date(') && resultValue.endsWith(')/')) {
                var timestamp = parseInt(resultValue.match(/\/Date\((\d+)\)\//)[1], 10);
                var formattedDate = new Date(timestamp).toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
                NWF$('#' + trgtID[index]).val(formattedDate);
            } else {
                NWF$('#' + trgtID[index]).val(resultValue);
            }
        });
    }

    popUpDialog.dialog("close");

    options = [];
    tblHeaders = [];
    tblName = "";
    trgtID = '';
    trgtCol = "";
}
