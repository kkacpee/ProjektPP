// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).on("click", ".edit-btn", function (event) {
    console.log("siema");
    $('.' + event.target.id + '.toglable').toggleClass('d-none');
});

$(document).on("click", ".filtersShow", function (event) {
    console.log("siema");
    $('.filtersForm').toggleClass('d-none');
});


$(document).on("click", ".b-filters", function (event) {
    console.log("siema");
    $('.rotate').toggleClass('down');
    if ($('.b-search').height() < 50) {
        $('.s-filters').toggleClass("s-filters-h");
        $('.b-search').animate({ height: "80px" }, 500);   
    }
    else {
        $('.b-search').animate({ height: "40px" }, 500, function () {
            $('.s-filters').toggleClass("s-filters-h")
        });
    }

});

$.wait = function (callback, seconds) {
    return window.setTimeout(callback, seconds * 1000);
}

$.wait(function () {
    $('.redirect').submit();
}, 30)



