$(document).ready(function () {
    $("tr").bind("click", function () {
        $(this).toggleClass("hang");
    })
    $(document).keypress(function (evt) {
        if (evt.keyCode == 27)
            $(".hang").removeAttr("class");
    });
})