/// <reference path="jquery-1.8.2.js" />
/// <reference path="jquery-ui-1.8.24.js" />

//#region 窗体显示在页面中间
jQuery.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
};
//窗体form位置
function Z_CenterForm(form) {
    form.closest('div.ui-jqdialog').center();
}

// search form
function centerViewForm(form) {
    var viewform = "#searchmodfbox_" + $.jgrid.jqID(this.id);
    form.closest(viewform).center();
}
//#endregion 

//#region 获得数据Id
function Z_GetId() {
    // get selected row's field (etc. Id,MRPCn,MRPCnName)
    var selectedRow = $('#list').getGridParam("selrow");
    return {
        myId: ($('#list').getRowData(selectedRow)).Id
    };
}

//#endregion

//#region 高亮选中行
function Z_hlSelectRow() {
    var selectRow = $("#list").jqGrid('getGridParam', 'selrow');
    //$("#list").jqGrid('setGridParam', { datatype: 'json' }).trigger('reloadGrid'); //if $(this)页面就没有刷新，如果有空格就会显示出数据前的空格（实际已去掉空格）。
    setTimeout(function () {
        $("#list").jqGrid('setSelection', selectRow);
    }, 100);
}
//#endregion 

//#region jqGrid事件
/*
名称：afterSubmit
说明：Add和Delete操作之后保留窗口，便于使用者连续添加或删除。Edit成功后窗口自动消失，失败则显示提示信息。
时间：2013.9.13
*/

// Edit/Add：编辑和添加可以共用
function Z_AfterSubmit(response, postdata) {
    var DialogVars = $.parseJSON(response.responseText);//响应信息
    $infoTr = $("#TblGrid_" + $.jgrid.jqID(this.id) + ">tbody>tr.tinfo"),
    $infoTd = $infoTr.children("td.topinfo");//信息栏
    if (DialogVars.success) {
        var myInfo = '<div class="ui-state-highlight ui-corner-all">' +
                        '<span class="ui-icon ui-icon-info" style="float: left; margin-right: .3em;"></span>' +
                        DialogVars.message +
                     '</div>'
        $infoTd.html(myInfo);
        $infoTr.show();
        //display status message to 3 sec only
        setTimeout(function () {
            //$infoTr.slideUp("slow");
            $('.tinfo').slideUp("slow");
        }, 2000);
        //setTimeout(function(){
        //    $(".ui-jqdialog-titlebar-close").click();
        //},1000);
        return [true, myInfo, ""];
    } else {
        var myInfo = //'<div class="ui-state-highlight ui-corner-all">' +
                     '<span class="ui-icon ui-icon-alert" style="float: left; margin-right: .3em;"></span>' +
                        DialogVars.message +
                    '</div>'
        //错误信息是在id为FormError的单元格显示
        $infoTr.hide();//隐藏信息栏以免同时显示info和FormError
        return [false, myInfo, ""];
    }
}

//afterComplete 编辑完成后保持高亮行
function Z_AfterComplete_Edit(response, postdata) {
    $('#list').jqGrid('setSelection', postdata.id);
}
//#endregion

//#region 日历选择
function datePick(elem) {
    $.datepicker.setDefaults($.datepicker.regional['zh-CN']);
    $(elem).datepicker({ dateFormat: "yy-mm-dd", changeMonth: true, changeYear: true, showMonthAfterYear: true });
    $("#ui-datepicker-div").css('font-size', '0.8em'); //改变大小
}
//#endregion

//#region 判断form表单里的文本框是否有输入
function Z_CheckFormInput() {
    var i = 0;
    $("form :text").each(function () {
        if ($.trim($(this).val()) != "")
            i += 1;
    });
    //alert(i)    
    return i;
}
//#endregion

//#region 清空form表单里的文本框内容
function Z_ClearFormInput() {
    $("form :text").each(function () {
        $(this).val("");
    });
}
//#endregion

//#region IE6 下拉菜单
//ie6hover = function () {
//    var nav_lis = document.getElementById("nav").getElementsByTagName("li");
//    for (var i = 0; i < nav_lis.length; i++) {
//        nav_lis[i].onmouseover = function () {
//            this.className += " ie6hover";
//        }
//        nav_lis[i].onmouseout = function () {
//            this.className = this.className.replace(new RegExp(" ie6hover\\b"), "");
//        }
//    }
//}

//if (window.attachEvent) {
//    window.attachEvent("onload", ie6hover);
//}
//#endregion

//#region 自动完成功能 Autocomplete
$(document).ready(function () {
    $(":input[data-autocomplete]").each(function () {
        $(this).autocomplete({ source: $(this).attr("data-autocomplete"), autoFocus: true });
    });
});
//#endregion

//BOM:图号
function numberAuto3(e) {
    $(e).autocomplete({
        source: function (request, response) {
            $.post("/BOM/QuickSearch", { data: request.term }, function (data) {
                response($.map(data, function (item) {
                    return item.label;
                }))
            }, "json");
        },
        minLength: 2,
        dataType: "json",
        cache: false,
        focus: function (event, ui) {
            return false;
        },
        select: function (event, ui) {
            this.value = ui.item.label;
            /* Do something with user_id */
            return false;
        }
    });
}

function numberAuto_4(e) {
    $(e).autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/BOM/QuickSearch",
                data: request,
                success: function (data) {
                    response(data);
                    if (data.length === 0) {
                        // Do logic for empty result.
                        alert('没有输入的这个数据');
                    }
                },
                error: function () {
                    response([]);
                }
            });
        },
        focus: function (event, ui) {
        //return false;
    },
    select: function (event, ui) {
        this.value = ui.item.label;
        /* Do something with user_id */
        $(this).val(ui.item.label);
        return false;
    }
    });
}

//#region 图号-自动完成
function numberAuto(e) {
    $(e).autocomplete(
        {
            source: '/Autocomplete/QuickSearch',
            autoFocus: false,
            delay: 0,
            select: function (event, ui) {
                this.value = ui.item.value;
                $(this).trigger('change');
                return false;
            }
    });//autocomplete ends
}
//#endregion

//#region 产品（父项）图号-自动完成
function pnumberAuto(e) {
    $(e).autocomplete({
        source: '/Autocomplete/QuickSearchPN',
        delay:0,
        select: function (event, ui) {
            this.value = ui.item.value;
            $(this).trigger('change');
            return false;
        }
    })
}
//#endregion
//#region 零件（子项）图号-自动完成
function cnumberAuto(e) {
    $(e).autocomplete({
        source: '/Autocomplete/QuickSearchCN',
        delay: 0,
        select: function (event, ui) {
            this.value = ui.item.value;
            $(this).trigger('change');
            return false;
        }
    })
}
//#endregion

//#region 物料代码-自动完成
function matnrAuto(elem) {
    $(elem).autocomplete({
        source: '/Autocomplete/QuickSearchMatNR',
        delay:0
    })
}
//#endregion

//#region 物料描述-自动完成
function matdbAuto(elem) {
    $(elem).autocomplete({
        source: '/Autocomplete/QuickSearchMatDB',
        delay: 0
    })
}
//#endregion

//#region 生产厂-自动完成
function factoryAuto(elem) {
    $(elem).autocomplete({
        source: '/Autocomplete/QuickSearchFactory',
        delay: 0,
        select: function (event, ui) {
            this.value = ui.item.value;
            $(this).trigger('change');
            return false;
        }
    })
}
//#endregion

//#region 工作中心代码-自动完成
function workCenterAuto(elem) {
    $(elem).autocomplete({
        source: "/Autocomplete/QuickSearchWorkCenter", delay: 0,
        select: function (event, ui) {
            $(this).val(ui.item.label)
        }
    })
}
//#endregion

//#region 工作中心描述-自动完成
function workCenterNameAuto(elem) {
    $(elem).autocomplete({
        source: "/Autocomplete/QuickSearchWorkCenterName", delay: 0,
        select: function (event, ui) {
            $(this).val(ui.item.label)
        }
    })
}
//#endregion

//#region 成本中心描述-自动完成
function costCenterAuto(elem) {
    $(elem).autocomplete({
        source: "/Autocomplete/QuickSearchCostCenter", delay: 0,
        select: function (event, ui) {
            $(this).val(ui.item.label)
        }
    })
}
//#endregion