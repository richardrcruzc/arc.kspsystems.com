$(function() {
    'use strict'; // Start of use strict
    /*------------------------------------------------------------------
     Progress Bar
     ------------------------------------------------------------------*/
    $('.progress .progress-bar').css("width",
            function() {
                return $(this).attr("aria-valuenow") + "%";
            }
        )
        /*------------------------------------------------------------------
             Header Search
        ------------------------------------------------------------------*/
    $("#search-form").hide();
    $(".fa-search").on('click', function() {
        $("#search-form").toggle();
        $("#search-form").fadeIn().addClass("open");
    });
    $("#search-form .close").on('click', function() {
        $("#search-form").fadeOut().removeClass("open");
        $("#this").hide();
    });
    /*------------------------------------------------------------------
     Scroll Top
     ------------------------------------------------------------------*/
    $.scrollUp({
        scrollText: '<i class="fa fa-angle-up"></i>',
        easingType: 'linear',
        scrollSpeed: 900,
        animation: 'fade'
    });
    /*------------------------------------------------------------------
    Header Navigation
    ------------------------------------------------------------------*/
    var windowSize = $(window).width();

    if (windowSize >= 767) {
        $('ul.nav li.dropdown').hover(function() {
            $(this).find('.dropdown-menu').stop(true, true).delay(200).fadeIn(500);
        }, function() {
            $(this).find('.dropdown-menu').stop(true, true).delay(200).fadeOut(500);
        });
    }
});
/*------------------------------------------------------------------
Classic Accordion
------------------------------------------------------------------*/
(function($) {
    'use strict'; // Start of use strict
    // Accordion Toggle Items
    var iconOpen = 'fa fa-minus',
        iconClose = 'fa fa-plus';

    $(document).on('show.bs.collapse hide.bs.collapse', '.accordion', function(e) {
        var $target = $(e.target)
        $target.siblings('.accordion-heading')
            .find('em').toggleClass(iconOpen + ' ' + iconClose);
        if (e.type == 'show')
            $target.prev('.accordion-heading').find('.accordion-toggle').addClass('active');
        if (e.type == 'hide')
            $(this).find('.accordion-toggle').not($target).removeClass('active');
    });

})(jQuery);
/*---------------------
    Circular Bars - Knob
    --------------------- */
if (typeof($.fn.knob) != 'undefined') {
    $('.knob').each(function() {
        var $this = $(this),
            knobVal = $this.attr('data-rel');

        $this.knob({
            'draw': function() {
                $(this.i).val(this.cv + '%')
            }
        });
        $this.appear(function() {
            $({
                value: 0
            }).animate({
                value: knobVal
            }, {
                duration: 2000,
                easing: 'swing',
                step: function() {
                    $this.val(Math.ceil(this.value)).trigger('change');
                }
            });
        }, {
            accX: 0,
            accY: -150
        });
    });
}
/*------------------------------------------------------------------
WOW
------------------------------------------------------------------*/
wow = new WOW({
    animateClass: 'animated',
    offset: 100
});
wow.init();
/*------------------------------------------------------------------
 Loader 
------------------------------------------------------------------*/
jQuery(window).on("load scroll", function() {
    'use strict'; // Start of use strict
    // Loader 
    $("#dvLoading").fadeOut("fast");
    //Animation Numbers	 
    jQuery('.animateNumber').each(function() {
        var num = jQuery(this).attr('data-num');
        var top = jQuery(document).scrollTop() + (jQuery(window).height());
        var pos_top = jQuery(this).offset().top;
        if (top > pos_top && !jQuery(this).hasClass('active')) {
            jQuery(this).addClass('active').animateNumber({
                number: num
            }, 2000);
        }
    });
/*------------------------------------------------------------------
Count Down
------------------------------------------------------------------*/
//$('#jcountdown').countdown('2018/06/13 00:00:00', function(event) {
//    var $this = $(this).html(event.strftime(''
//      + '<span class="countdown-row countdown-show4"><span class="countdown-section"><span class="countdown-amount">%-D</span> <span class="countdown-period">days</span></span>'
//      + '<span class="countdown-section"><span class="countdown-amount">%H</span> <span class="countdown-period">Hours</span></span>'
//      + '<span class="countdown-section"><span class="countdown-amount">%M</span> <span class="countdown-period">Minutes</span></span>'
//      + '<span class="countdown-section"><span class="countdown-amount">%S</span> <span class="countdown-period">Seconds</span></span></span>'));
//  });
});

