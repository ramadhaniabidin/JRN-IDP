let options = [];
let tblHeaders = [];
let tblName = "";
let moduleName = "";
let trgtID = "";
let trgtCol = [];
let page_count = 0;
let detailTable = "";
let ColumnToFilter = "";
let FilterValue = "";
let targetID_Detail = [];
let targetCol_Detail = [];
let popUpDialog;
let page_CurrIdx = 1;
let page_Count = 1;
let dataResult = [];
let dataResult_Details = [];
let inputSelectors = [];
let transNumber = '';
let selectedItem = [];
let sumTotalValue = 0;
let vendorName;
let itemsRS_currSelected;

NWF.FormFiller.Events.RegisterAfterReady(function () {
    defaultOnloadForm();
    deleteAllRows();
    NWF$("#" + PIB).change(function () {
        onChangePIB();
    });
    NWF$("#" + BPNPaidBy).change(function () {
        onchangeBPNPaidBy();
    });
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "" && $('#' + ItemID).val() != "Revise") {
        attachmentOpenNewTab();
    }
    RepeatingSectionPopUp();
    GetWorkflowHistoryLog();
    SetAttachmentLink();
});


NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    defaultOnloadForm();
    RepeatingSectionPopUp();
    GetWorkflowHistoryLog();
    SetAttachmentLink();
});

function SetAttachmentLink() {
    if (NWF$('#' + cvFormStatus).val() === '' || NWF$('#' + cvFormStatus).val() === 'Revise' || NWF$('#' + cvFormStatus).val() === 'Draft') {
        $(".nf-attachmentsLink").css("display", "block");
        $(".propertysheet").css("display", "block");
    } else {
        $(".nf-attachmentsLink").css("display", "none");
        $(".propertysheet").css("display", "none");
        $(".nf-attachmentsRow a").attr("target", "_blank");
    }
};

function RepeatingSectionPopUp() {
    const RS = document.querySelectorAll(".ANCRS .nf-repeater-row:not(.nf-repeater-row-hidden)");
    RS.forEach(function (row) {
        const popUpButtons = row.querySelectorAll(".pop-up");
        popUpButtons.forEach(function (btn) {
            btn.addEventListener("click", function () {
                PopUp_ShowDialog(null, "ANC Vendor Bank RS", row);
            });
        });
    });
};

function PopUp_GenerateTableHeaders(module) {
    if (module === "ANC Vendor Bank" || module === "ANC Vendor Bank RS") {
        tblHeaders = [
            { db_col: "Vendor_Name", name: "Vendor Name" },
            { db_col: "Vendor_Number", name: "Vendor Number" },
            { db_col: "Bank_Key", name: "Bank Key" },
            { db_col: "Bank_Account_No", name: "Account No" },
            { db_col: "Bank_Account_Name", name: "Account Name" },
            { db_col: "Bank_Name", name: "Bank Name" },
            { db_col: "Partner_Bank", name: "Partner Bank" },
        ];
    }
};

function PopUp_GenerateOptions(module) {
    if (module === "ANC Vendor Bank" || module === "ANC Vendor Bank RS") {
        options = [
            { value: "Vendor_Name", name: "Vendor Name" },
            { value: "Vendor_Number", name: "Vendor Number" },
            { value: "Bank_Key", name: "Bank Key" },
            { value: "Bank_Account_No", name: "Account No" },
            { value: "Bank_Account_Name", name: "Account Name" },
            { value: "Bank_Name", name: "Bank Name" },
            { value: "Partner_Bank", name: "Partner Bank" },
        ];
    }
};

function PopUp_ShowDialog(shownPopUp, module, currentRow) {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 650,
        width: 1000,
        title: "Select : " + module
    });
    moduleName = module;
    PopUp_GenerateTableHeaders(module);
    PopUp_GenerateOptions(module);
    PopUp_GetTargetIDnColumns(module);
    if (currentRow !== null) {
        itemsRS_currSelected = currentRow;
    }
    PopUp_PopulateOptions();
    PopUp_PopulateTableHeaders();
    PopUp_Search();
};

function PopUp_Search() {
    page_CurrIdx = 1;
    PopUp_List(page_CurrIdx, $("#PopUp_Dropdown").val(), $("#PopUp_Keyword").val(), selectedItem);
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

function PopUp_List(PageIndex, SearchBy, Keywords, SelectedItem) {
    try {
        Keywords = PopUp_ValidateKeywords(Keywords, moduleName);
        SearchBy = PopUp_ValidateSearchBy(SearchBy, moduleName);
        const param = PopUp_GenerateParam(tblName, SearchBy, Keywords, PageIndex);
        LoadPopUpData(param);
    }
    catch (err) {
        console.log("PopUp Error: PopUp_List() \n" + err);
    }
};

function GetWorkflowHistoryLog() {
    try {
        const objek = {
            Form_No: NWF$('#' + textFormNo).val(),
            Transaction_ID: NWF$("#" + cvItemID).val(),
            Module_Code: NWF$("#" + cvModuleCode).val()
        };
        LoadHistoryLog(objek);
    }
    catch (err) {
        console.log('GetWorkflowHistoryList : ' + err.message);
    }
};

async function LoadHistoryLog(param) {
    try {
        const response = await fetch("/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetHistoryLog2", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(param)
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const jsonData = JSON.parse(data.d);
        HandleHistoryLogSuccess(jsonData);
    }
    catch (err) {
        console.log(err);
    }
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

function HandleHistoryLogSuccess(jsonData) {
    let trHTML = "";
    const tblHistory = document.getElementById("tblHistory");
    const tbody = tblHistory.querySelector("tbody");
    const rows = tblHistory.querySelectorAll("tr:not(:first-child)");
    rows.forEach(row => row.remove());
    jsonData.forEach(item => {
        trHTML += `<tr>
            <td>${item.Personal_Name}</td>
            <td>${item.Position}</td>
            <td>${item.Comments}</td>
            <td>${item.Action_Name}</td>
            <td>${item.Action_Date}</td>
        </tr>`
    });
    tbody.insertAdjacentHTML('beforeend', trHTML);
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

function SearchHelper(event) {
    if (event.key === 'Enter') {
        PopUp_Search();
    }
};

function PopUp_SelectItem(index) {
    trgtCol.forEach((col, idx) => {
        const fieldValue = PopUpSelectFormatValue(index, col);
        if (moduleName === "ANC Vendor Bank") {
            NWF$("#" + trgtID[idx]).val(fieldValue);
        }
        else if (moduleName === "ANC Vendor Bank RS") {
            if (itemsRS_currSelected !== null && itemsRS_currSelected !== undefined) {
                const input = itemsRS_currSelected.querySelector("." + trgtID[idx] + " input");
                input.value = fieldValue;
                input.dispatchEvent(new Event("click"));
                input.dispatchEvent(new Event("blur"));
            }
            itemsRS_currSelected = null;
        }
    });
    ClosePopUp();
};

function ClosePopUp() {
    popUpDialog.dialog("close");
    options = [];
    tblHeaders = [];
    tblName = "";
    trgtID = "";
    trgtCol = "";
};

function PopUpSelectFormatValue(index, key) {
    const selected = dataResult[index].filter(x => x.Key === key)[0];
    if (selected === null || selected === undefined) {
        return "";
    }
    return FormatValue(selected.Value);
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

function GenerateDataColumn(value) {
    let dataCol = document.createElement("td");
    dataCol.setAttribute("colspan", 3);
    dataCol.setAttribute("style", "text-align:center");
    dataCol.innerHTML = value;
    return dataCol;
};

function PopUp_ValidateKeywords(keywords, moduleName) {
    if ((keywords === null) || (keywords === undefined)) {
        keywords = "";
    }
    if (moduleName === "ANC Vendor Bank" || moduleName === "ANC Vendor Bank RS") {
        keywords += ";bea cukai;1";
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
    return searchBy;
};

function PopUp_GetTargetIDnColumns(module) {
    if (module === "ANC Vendor Bank") {
        tblName = "MasterVendorBankAffiliate";
        trgtID = [VendorName, VendorNumber, BankKey, BankAccountNo, BankAccountName, BankName, PartnerBank];
        trgtCol = ["Vendor_Name", "Vendor_Number", "Bank_Key", "Bank_Account_No", "Bank_Account_Name", "Bank_Name", "Partner_Bank"];
    } else if (module === "ANC Vendor Bank RS") {
        tblName = "MasterVendorBankAffiliate";
        trgtID = ["VendorName"];
        trgtCol = ["Vendor_Name"];
    }
};

function PopUp_PopulateOptions() {
    $('#PopUp_Dropdown').val('');
    $('#PopUp_Keyword').val('');
    $('#PopUp_Dropdown').html('');
    $('#PopUp_TableBody').html('');
    $.each(options, function (i, option) {
        $("#PopUp_Dropdown").append($("<option>", {
            text: option.name,
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

function attachmentOpenNewTab() {
    NWF$('.atcNewTab').click(function () {
        NWF$('tbody tr td a').attr("target", "_blank");
    });
};

function defaultOnloadForm() {
    $("#" + Comment).val("");
    $('.btnApproval input[type="radio"]').prop('checked', false);
    NWF$("#s4-ribbonrow").hide();
};

function deleteRepeaterRow(rowNo) {
    let row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(rowNo)
    row.find(".nf-repeater-deleterow-image").css("display", "none");
};

function deleteAllRows() {
    deleteRepeaterRow(0);
    deleteRepeaterRow(1);
    deleteRepeaterRow(2);
    deleteRepeaterRow(3);
};

function deleteAmountAffiliate(rowNo) {
    let row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(rowNo);
    row.find(".taxBase input").val('');
    row.find(".amount input").val('');
    row.find(".amount label").html('');
};

function deleteAllAmountAffiliate() {
    deleteAmountAffiliate(0);
    deleteAmountAffiliate(1);
    deleteAmountAffiliate(2);
    deleteAmountAffiliate(3);
};


function onChangePIB() {
    let PIB = NWF$('.PIB label').html();
    if (PIB === "No") {
        deleteAllAmountAffiliate();
    }
};

function onchangeBPNPaidBy() {
    let BPNPaid = NWF$("#" + BPNPaidBy).val();
    if (typeof (BPNPaid) === "string" && BPNPaid.includes("Vendor")) {
        deleteAmountAffiliate(1);
        deleteAmountAffiliate(2);
    } else if (typeof (BPNPaid) === "string" && BPNPaid.includes("Daikin")) {
        deleteAmountAffiliate(3);
    }
};