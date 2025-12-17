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
let totalItemCnt;

const PopUp_ColumnMapping = {
    "ANC Vendor Bank": {
        tableName: "MasterVendorBankAffiliate",
        targetColumns: ["Vendor_Name", "Vendor_Number", "Bank_Key", "Bank_Account_No", "Bank_Account_Name", "Bank_Name", "Partner_Bank"],
        // targetID: [VendorName, VendorNumber, BankKey, BankAccountNo, BankAccountName, BankName, PartnerBank]
        targetID: []
    },
    "ANC Vendor Bank RS": {
        tableName: "MasterVendorBankAffiliate",
        targetColumns: ["Vendor_Name"],
        targetID: ["VendorName"]
    },
    "Material Anaplan": {
        tableName: "MasterMaterialAnaplan",
        targetColumns: ["Concatenate_GA"],
        targetID: ["mat_anaplan"]
    },
    "Cost Center": {
        tableName: "MasterMappingCostCenter",
        targetColumns: ["Combine", "Cost_Center"],
        targetID: ["cost-center", "cc_code"]
    },
    "": {}
};

const PopUp_TableHeadersMapping = {
    "ANC Vendor Bank": [
        { db_col: "Vendor_Name", name: "Vendor Name" },
        { db_col: "Vendor_Number", name: "Vendor Number" },
        { db_col: "Bank_Key", name: "Bank Key" },
        { db_col: "Bank_Account_No", name: "Account No" },
        { db_col: "Bank_Account_Name", name: "Account Name" },
        { db_col: "Bank_Name", name: "Bank Name" },
        { db_col: "Partner_Bank", name: "Partner Bank" },
    ],
    "ANC Vendor Bank RS": [
        { db_col: "Vendor_Name", name: "Vendor Name" },
        { db_col: "Vendor_Number", name: "Vendor Number" },
        { db_col: "Bank_Key", name: "Bank Key" },
        { db_col: "Bank_Account_No", name: "Account No" },
        { db_col: "Bank_Account_Name", name: "Account Name" },
        { db_col: "Bank_Name", name: "Bank Name" },
        { db_col: "Partner_Bank", name: "Partner Bank" },
    ],
    "Material Anaplan": [
        { db_col: "Material_Code", name: "Kode Material" },
        { db_col: "Concatenate_GA", name: "Material" },
        { db_col: "Procurement_Department_Title", name: "Procurement Department" },
    ],
    "Cost Center": [
        { db_col: "Cost_Center", name: "Cost Center" },
        { db_col: "Description", name: "Description" },
        { db_col: "Branch", name: "Branch" },
        { db_col: "Combine", name: "Combine" },
    ],
    "": []
};

const PopUp_OptionsMapping = {
    "ANC Vendor Bank": [
        { value: "Vendor_Name", name: "Vendor Name" },
        { value: "Vendor_Number", name: "Vendor Number" },
        { value: "Bank_Key", name: "Bank Key" },
        { value: "Bank_Account_No", name: "Account No" },
        { value: "Bank_Account_Name", name: "Account Name" },
        { value: "Bank_Name", name: "Bank Name" },
        { value: "Partner_Bank", name: "Partner Bank" },
    ],
    "ANC Vendor Bank RS": [
        { value: "Vendor_Name", name: "Vendor Name" },
        { value: "Vendor_Number", name: "Vendor Number" },
        { value: "Bank_Key", name: "Bank Key" },
        { value: "Bank_Account_No", name: "Account No" },
        { value: "Bank_Account_Name", name: "Account Name" },
        { value: "Bank_Name", name: "Bank Name" },
        { value: "Partner_Bank", name: "Partner Bank" },
    ],
    "Material Anaplan": [
        { value: "Material_Code", name: "Kode Material" },
        { value: "Concatenate_GA", name: "Material" },
        { value: "Procurement_Department_Title", name: "Procurement Department" },
    ],
    "Cost Center": [
        { value: "Cost_Center", name: "Cost Center" },
        { value: "Description", name: "Description" },
        { value: "Branch", name: "Branch" },
        { value: "Combine", name: "Combine" },
    ],
    "": []
};

NWF.FormFiller.Events.RegisterAfterReady(function () {
    console.log("Masuk Condition Business Relation")
    const control = NWF$("#" + Approvers);
    control.val('Approver1; Approver2;');
    control.trigger('change')
    NWF.FormFiller.Functions.ProcessOnChange(control);
    $('#' + Comment).val("")
    $('.btnApproval input[type="radio"]').prop('checked', false);
    //NWF$("#RibbonContainer").hide();
    NWF$("#s4-ribbonrow").hide();
    //NWF$('.atcNewTab').click(function () {
    //    NWF$('tbody tr td a').attr("target", "_blank");
    //});
    if ($('#' + ListName).val() == "Affiliate Not Claim") {
        deleteRepeaterRow(0)
        deleteRepeaterRow(1)
        deleteRepeaterRow(2)
        deleteRepeaterRow(3)
        NWF$('#' + PIB).change(function () {
            const PIB = NWF$('.PIB label').html()
            console.log(PIB)
            if (PIB == "No") {
                deleteAmountAffiliate(0)
                deleteAmountAffiliate(1)
                deleteAmountAffiliate(2)
                deleteAmountAffiliate(3)
            }
        });
        NWF$('#' + BPNPaidBy).change(function () {
            const BPNPaid = NWF$('#' + BPNPaidBy).val()
            const PIB = NWF$('#' + PIB).val()
            if (BPNPaid.includes("Vendor")) {
                deleteAmountAffiliate(1)
                deleteAmountAffiliate(2)
            } else if (BPNPaid.includes("Daikin")) {
                deleteAmountAffiliate(3)
            }
        });

    }
    if ($('#' + ListName).val() == "Business Relation" && $('#' + ItemID).val() != null && $('#' + ItemID).val() != "") {
        console.log("Masuk Condition Business Relation")
        //NWF$('.atcNewTab').click(function () {
        //    NWF$('tbody tr td a').attr("target", "_blank");
        //});
        setAttachmentSelfie($('#' + ItemID).val())
    }
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "") {
        //console.log('Masuk Sini')
        const control = NWF$("#" + Approvers);
        control.val('Approver1; Approver2;');
        control.trigger('change')
        NWF.FormFiller.Functions.ProcessOnChange(control);
        //getApprover($('#' + ListName).val(), $('#' + ItemID).val())
        //NWF$('.atcNewTab').click(function () {
        //    NWF$('tbody tr td a').attr("target", "_blank");
        //});
    }
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "" && NWF$('#' + cvFormStatus).val() != 'Revise') {
        NWF$('.atcNewTab').click(function () {
            NWF$('tbody tr td a').attr("target", "_blank");
        });
    }

    const repeatingSection = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    repeatingSection.each(function () {
        const row = NWF$(this);
        NWF$(this).find('.pop-up').on('click', function () {
            PopUp_ShowDialog(null, "Material Anaplan", row);
        });
    });

    const poRS = NWF$(".poRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    poRS.each(function () {
        const poRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "Cost Center", poRSRow);
        });
    });

    const ANCRS = NWF$(".ANCRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    ANCRS.each(function () {
        const ANCRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "ANC Vendor Bank RS", ANCRSRow);
        });
    });

    //GetUserProfile();
});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    $('.btnApproval input[type="radio"]').prop('checked', false);
    const repeatingSection = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    repeatingSection.each(function () {
        const row = NWF$(this);
        NWF$(this).find('.pop-up').on('click', function () {
            PopUp_ShowDialog(null, "Material Anaplan", row);
        });
    });

    const poRS = NWF$(".poRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    poRS.each(function () {
        const poRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "Cost Center", poRSRow);
        });
    });

    const ANCRS = NWF$(".ANCRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    ANCRS.each(function () {
        const ANCRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "ANC Vendor Bank RS", ANCRSRow);
        });
    });
});

function GetUserProfile() {
    const param = {
        'logon': NWF$("#" + current_user).val()
        //logon: 'test1'
    };

    console.log(param);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetLoginAttributes",
        data: JSON.stringify(param),
        dataType: "json",
        async: true,
        success: function (data) {
            const jsonData = JSON.parse(data.d);
            console.log(jsonData);
            if (jsonData.Success) {
                $(".requester_name input").val(jsonData.CurrLoginName);
                $(".requester_name input").trigger("click");
                $(".requester_name input").trigger("blur");

                $(".requester_dept input").val(jsonData.Department);
                $(".requester_dept input").trigger("click");
                $(".requester_dept input").trigger("blur");
            }
        }
    });
};

function deleteAmountAffiliate(NoRow) {
    const row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(NoRow)
    row.find(".taxBase input").val('')
    row.find(".amount input").val('')
    row.find(".amount label").html('')
};

function deleteRepeaterRow(NoRow) {
    const row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(NoRow)
    row.find(".nf-repeater-deleterow-image").css("display", "none");
};

function setAttachmentSelfie(Item_ID) {
    const param = {
        Item_ID: Item_ID,
    };
    console.log(param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetAttachmentSelfie",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function (data) {
                const JSONData = JSON.parse(data.d)
                const atc = JSONData.dataATC
                console.log('AtcSelfie Data: ', atc)
                atc.forEach((dataAtc) => {
                    console.log(dataAtc, "dataperATC")
                    const url = `<a href="https://spdev:3473/Lists/Business%20Relation/Attachments/` + Item_ID + `/` + dataAtc.Attachment_Selfie + `" target="_blank"><img width="200px" src="http://spdev:3473/Lists/Business%20Relation/Attachments/` + Item_ID + `/` + dataAtc.Attachment_Selfie + `"></a>"`
                    console.log(url)
                    const row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(dataAtc.No - 1);
                    row.find(".atcselfietesting").html(url)
                });
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }
};

function getApprover(ListName, ListItemID) {
    const param = {
        ListName: ListName,
        ListItemID: ListItemID,
    };

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetApprover",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function (data) {
                console.log('Approver: ' + data.d)
                const control = NWF$("#" + Approvers);
                control.val(data.d);
                control.trigger('change')
                NWF.FormFiller.Functions.ProcessOnChange(control);
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }

}

function SaveApproval(approvalValue, ListName, ListItemID) {
    const param = {
        approvalValue: approvalValue,
        ListName: ListName,
        ListItemID: ListItemID,
        comments: NWF$('#' + Comment).val(),
    };

    console.log('Save & Submit Triggered.');

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequest",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function () {
                console.log('Your Response have been recorded!');
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }
};

function SaveApprovalNonCom(approvalValue, ListName, ListItemID, HeaderID) {
    const param = {
        approvalValue: approvalValue,
        ListName: ListName,
        ListItemID: ListItemID,
        HeaderID: HeaderID,
        comments: NWF$('#' + Comment).val(),
    };

    console.log('Save & Submit Triggered.');

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequestNonCom",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function () {
                console.log('Your Response have been recorded!');
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }

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
    return keywords;


    // if (moduleName == "Material Anaplan") {
    //     SearchBy += ";Procurement_Department_Title";
    //     Keywords += ";" + $(".proc_dept").find('input').val();
    // }

    // else if (moduleName == "Cost Center") {
    //     SearchBy += ";Branch";
    //     Keywords += ";" + $(".branch").find("label").html();
    // }

    // else if (moduleName === "ANC Vendor Bank") {
    //     SearchBy += ";Vendor_Name;Active";
    //     Keywords += ";bea cukai;1";
    // }

    // else if (moduleName === "ANC Vendor Bank RS") {
    //     SearchBy += ";Vendor_Name;Active";
    //     Keywords += ";bea cukai;1";
    // }
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
    return searchBy;
};

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

function PopUp_ShowDialog(shownPopup, module, currentRow) {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 650,
        width: 1000,
        title: "Select : " + module
    });
    PopUp_GetTargetTablenColumns(module);
    PopUp_GenerateTableHeaders(module);
    PopUp_GenerateOptions(module);
    PopUp_PopulateOptions();
    PopUp_PopulateTableHeaders();
    if (currentRow !== null) itemsRS_currSelected = currentRow;
    PopUp_Search();
};

function SearchHelper(event) {
    if (event.key === 'Enter') {
        PopUp_Search();
    }
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

function PopUp_List(PageIndex, SearchBy, Keywords, SelectedItem) {
    Keywords = PopUp_ValidateKeywords(Keywords, moduleName);
    SearchBy = PopUp_ValidateSearchBy(SearchBy, moduleName);
    const param = PopUp_GenerateParam(tblName, SearchBy, Keywords, PageIndex);
    console.log('Parameters: ', param);
    LoadPopUpData(param);
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

function PopUp_SelectItem(id) {
    console.log(dataResult[id]);
    if (moduleName == "Material Anaplan") {
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }
    else if (moduleName == "Cost Center") {
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }
    else if (moduleName == "ANC Vendor Bank") {
        $.each(trgtCol, function (index, values) {
            let resultValue;
            if ((dataResult[id].filter(e => e.Key == values)[0] !== null) && (dataResult[id].filter(e => e.Key == values)[0] !== undefined)) {
                resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;
            }

            else if ((dataResult[id].filter(e => e.Key == values)[0] == null) || (dataResult[id].filter(e => e.Key == values)[0] == undefined)) {
                resultValue = "";
            }
            console.log('606', resultValue, trgtID[index], moduleName)
            if (typeof resultValue === 'string' && resultValue.startsWith('/Date(') && resultValue.endsWith(')/')) {
                const timestamp = parseInt(resultValue.match(/\/Date\((\d+)\)\//)[1], 10);
                const formattedDate = new Date(timestamp).toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
                NWF$('#' + trgtID[index]).val(formattedDate);
            } else {
                NWF$('#' + trgtID[index]).val(resultValue);
            }
        });
    }
    else if (moduleName == "ANC Vendor Bank RS") {
        console.log('22222')
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            console.log('1111')
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }

    popUpDialog.dialog("close");

    options = [];
    tblHeaders = [];
    tblName = "";
    trgtID = '';
    trgtCol = "";
};