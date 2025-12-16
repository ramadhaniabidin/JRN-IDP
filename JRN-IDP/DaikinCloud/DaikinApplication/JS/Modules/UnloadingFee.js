

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
let itemsRS_currSelected;
let totalItemCnt;

const PopUp_ColumnMapping = {
    "UnloadingFee": {
        tableName: "MasterVendorUnloadingFee",
        targetColumns: ["Vendor_Name", "Title", "Bank_Account", "Account_Holder", "Payment_ID", "Bank_Key"],
        // targetID: [VendorName, VendorNumber, BankKey, BankAccountNo, BankAccountName, BankName, PartnerBank]
        targetID: []
    },
    "": {}
};

const PopUp_TableHeadersMapping = {
    "UnloadingFee": [
        { name: "Vendor Name", db_col: "Vendor_Name" },
        { name: "Vendor Number", db_col: "Title" },
        { name: "Payment ID", db_col: "Payment_ID" },
        { name: "Bank Account Number", db_col: "Bank_Account" },
        { name: "Bank Account Name", db_col: "Account_Holder" },
    ],
    "": []
};

const PopUp_OptionsMapping = {
    "UnloadingFee": [
        { value: "Vendor_Name", text: "Vendor Name" },
        { value: "Title", text: "Vendor Number" },
        { value: "Bank_Account", text: "Bank Account Number" },
        { value: "Account_Holder", text: "Bank Account Name" },
        { value: "Payment_ID", text: "Payment ID" },
    ],
    "": []
};

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

function PopUp_GetTargetTablenColumns(module) {
    const key = module ? module : "";
    const mapping = PopUp_ColumnMapping[key];
    tblName = mapping["tableName"];
    trgtCol = mapping["targetColumns"];
    trgtID = mapping["targetID"];
    moduleName = module;
};

function PopUp_GenerateTableHeaders(module) {
    const key = module ? module : "";
    tblHeaders = PopUp_TableHeadersMapping[key];
};

function PopUp_GenerateOptions(module) {
    const key = module ? module : "";
    options = PopUp_OptionsMapping[key];
};

function PopUp_GetTargetID(module) {
    if (module === "UnloadingFee") {
        trgtID = [VendorName, VendorNumber, AccountNumber, AccountName, PaymentID, BankKeyID];
    }
};

function PopUp_PopulateOptions() {
    $('#PopUp_Dropdown').val('');
    $('#PopUp_Keyword').val('');
    $('#PopUp_Dropdown').html('');
    $('#PopUp_TableBody').html('');
    $.each(options, function (i, option) {
        $("#PopUp_Dropdown").append($("<option>", {
            text: option.text,
            value: option.value
        }))
    });
};

function PopUp_PopulateTableHeaders() {
    $('#PopUp_TblHeader').html('');
    $.each(tblHeaders, function (i, header) {
        $("#PopUp_TblHeader").append($("<th>", {
            scope: "col",
            style: "text-align: center; color: white; background-color: #0072c6; padding: 10px;",
            text: header.name,
            colspan: 3
        }));
    });
    $('#PopUp_TblHeader').append($('<th>', {
        scope: "col",
        style: "text-align: center; color: white; background-color: #0072c6; padding: 10px;",
        name: "",
        colspan: 2
    }));
};

function PopUp_ValidateKeywords(keywords, moduleName) {
    if ((keywords === null) || (keywords === undefined)) {
        keywords = "";
    }
    if (moduleName === "ANC Vendor Bank" || moduleName === "ANC Vendor Bank RS") {
        keywords += ";bea cukai;1";
    }
    if (moduleName === "Cost Center") {
        keywords += ";" + $(".branch").find("label").html();
    }
    if (moduleName == "Material Anaplan") {
        keywords += ";" + $(".proc_dept").find('input').val();
    }
    if (moduleName === "Supplier") {
        keywords += ";Supplier";
    }
    return keywords;
};

function PopUp_ValidateSearchBy(searchBy, moduleName) {
    if (searchBy === null || searchBy === undefined) {
        searchBy = "";
    }
    if (moduleName === "ANC Vendor Bank" || moduleName === "ANC Vendor Bank RS") {
        searchBy += ";Vendor_Name;Active";
    }
    if (moduleName === "Material Anaplan") {
        searchBy += ";Procurement_Department_Title";
    }
    if (moduleName === "Cost Center") {
        searchBy += ";Branch";
    }
    if (moduleName === "Supplier") {
        searchBy += ";business_partner_category_name";
    }
    return searchBy;
};

function PopUp_ShowDialog(shownPopup, module, currentRow) {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 650,
        width: 1000,
        title: "Select : " + module
    });

    PopUp_GetTargetTablenColumns(module);
    PopUp_GetTargetID(module);
    PopUp_GenerateTableHeaders(module);
    PopUp_GenerateOptions(module);
    PopUp_PopulateOptions();
    PopUp_PopulateTableHeaders();
    if (currentRow != null) itemsRS_currSelected = currentRow;
    PopUp_Search();
};

function PopUp_Search() {
    page_CurrIdx = 1;
    PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
};

function PopUp_Prev() {
    if (page_CurrIdx > 1) {
        page_CurrIdx = page_CurrIdx - 1;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
};

function PopUp_Next() {
    if (page_CurrIdx < page_count) {
        page_CurrIdx++;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
};

function PopUp_GenerateParam(tableName, searchBy, keywords, pageIndex) {
    const param = {
        input: {
            searchTabl: tableName,
            searchCol: searchBy,
            searchVal: keywords,
            searchLike: 1,
            pageIndx: pageIndex,
            pageSize: 10,
        },
        output: {
            RecordCount: 0,
        }
    };

    return param;
};

async function LoadPopUpData(param) {
    const tBodyHTML = document.getElementById("PopUp_TableBody");
    try {
        const response = await fetch("_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(param)
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        HandlePopUpSuccess(data, param, tBodyHTML);
    } catch (error) {
        console.log(error);
    }
};

function FormatValue(value) {
    if (typeof value === "string" && value.includes("/Date(")) {
        const timestamp = parseInt(value.substring(6, 19), 10);
        return new Date(timestamp).toLocaleString();
    }
    if (typeof value === "string") {
        return value.trim();
    }
    return value;
};

function HandlePopUpSuccess(data, param, tBodyHTML) {
    tBodyHTML.innerHTML = "";
    const jsonData = JSON.parse(data.d);
    const logs = jsonData.Logs || [];
    dataResult = logs;
    logs.forEach((rowValues, index) => {
        const dataRow = document.createElement("tr");
        tblHeaders.forEach((header, _) => {
            const valueObj = rowValues.filter(x => x.Key === header.db_col)[0];
            const value = FormatValue(valueObj ? valueObj.Value : "");
            const dataCol = GenerateDataColumn(value);
            dataRow.appendChild(dataCol);
        });
        GenerateSelectColumn(dataRow, index, tBodyHTML);
    });
    DisplayPagination(parseInt(jsonData.TotalRecords || 0), param);
};

function GenerateDataColumn(value) {
    let dataCol = document.createElement("td");
    dataCol.setAttribute("colspan", 3);
    dataCol.setAttribute("style", "text-align:center");
    dataCol.innerHTML = value;
    return dataCol;
};

function DisplayPagination(totalItems, param) {
    const pageCount = Math.ceil(totalItems / param.input.pageSize);
    page_count = pageCount;
    $("#info-paging_items").html(
        `Page: ${param.input.pageIndx} of ${pageCount} | ${totalItems} Results`
    );
};

function GenerateSelectColumn(dataRow, index, tBody) {
    let colSelect = document.createElement("td");
    colSelect.setAttribute("colspan", 2);
    colSelect.setAttribute("style", "text-align:center;cursor:pointer;");
    colSelect.innerHTML = `<a style="color:blue;" class="action-name" onclick="PopUp_SelectItem(${index})">SELECT</a>`;
    dataRow.appendChild(colSelect);
    tBody.appendChild(dataRow);
};

function PopUp_List(PageIndex, SearchBy, Keywords, SelectedItem) {
    Keywords = PopUp_ValidateKeywords(Keywords, moduleName);
    SearchBy = PopUp_ValidateSearchBy(SearchBy, moduleName);
    const param = PopUp_GenerateParam(tblName, SearchBy, Keywords, PageIndex);
    LoadPopUpData(param);
};

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
