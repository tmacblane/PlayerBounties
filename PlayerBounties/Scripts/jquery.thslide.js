/*=============================    
jquery.thslide.js 
v.0.1
Julien Decaudin - 04/2010
www.juliendecaudin.com
=============================*/

(function ($) {
    $.fn.thslide = function (callerSettings) {
        var settings = $.extend({
            navPreviousWrapper: '.thslide_nav_previous a',
            navNextWrapper: '.thslide_nav_next a',
            listWrapper: '.thslide_list ul',
            itemOffset: 100, //distance in pixel between the left-hand side of an item and the following one
            itemVisible: 5, //number of items visible by the user
            slideSpeedSlow: 600, //speed of the items when sliding slowly
            slideSpeedFast: 200, //speed of the items when sliding fast            
            infiniteScroll: 0, //scroll infinitely through items (deactivated by default)
            scrollOver: 0, //scroll on rollover
            debug: 0
        }, callerSettings || {});

        settings.navPreviousWrapper = $(this).find(settings.navPreviousWrapper);
        settings.navNextWrapper = $(this).find(settings.navNextWrapper);
        settings.listWrapper = $(this).find(settings.listWrapper);
        settings.totalItem = $(settings.listWrapper).find('li').length; //total number of items
        settings.itemOffsetMax;
        settings.listMarginLeft;
        settings.slideSpeed;
        settings.easing;
        settings.locked = false;
        settings.scrollOn = false;
        settings.scrollTimer = 0;
        settings.navTimer;

        if (settings.totalItem > settings.itemVisible) {
            // --- Init list & items offset
            updateListMargin(settings);
            settings.itemOffsetMax = parseInt($(settings.listWrapper).find('li').length) * settings.itemOffset - (settings.itemVisible * settings.itemOffset);
            settings.itemOffsetMax = -parseInt(settings.itemOffsetMax);

            // --- Init interactions
            //click
            $(settings.navPreviousWrapper).click(function () {
                return false;
            });

            $(settings.navNextWrapper).click(function () {
                return false;
            });

            //mouse down
            $(settings.navPreviousWrapper).mousedown(function () {
                window.clearTimeout(settings.navTimer);
                settings.scrollOn = true;
                settings.slideSpeed = settings.slideSpeedFast;
                settings.easing = "swing";
                slideListPrevious(settings);
            });

            $(settings.navNextWrapper).mousedown(function () {
                window.clearTimeout(settings.navTimer);
                settings.scrollOn = true;
                settings.slideSpeed = settings.slideSpeedFast;
                settings.easing = "swing";
                slideListNext(settings);
            });

            //mouse up
            $(settings.navPreviousWrapper).mouseup(function () {
                settings.scrollOn = false;
            });

            $(settings.navNextWrapper).mouseup(function () {
                settings.scrollOn = false;
            });

            if (settings.scrollOver == 1) {
                //mouse over (rollover)
                $(settings.navPreviousWrapper).mouseover(function () {
                    //if the nav isn't already scrolling
                    if (!settings.scrollOn) {
                        settings.scrollOn = true;
                        settings.slideSpeed = settings.slideSpeedSlow;
                        settings.easing = "linear";

                        var functionCall = function () { slideListPrevious(settings); };
                        settings.navTimer = window.setTimeout(functionCall, settings.scrollTimer);
                    }
                });

                $(settings.navNextWrapper).mouseover(function () {
                    //if the nav isn't already scrolling
                    if (!settings.scrollOn) {
                        settings.scrollOn = true;
                        settings.slideSpeed = settings.slideSpeedSlow;
                        settings.easing = "linear";

                        var functionCall = function () { slideListNext(settings); };
                        settings.navTimer = window.setTimeout(functionCall, settings.scrollTimer);
                    }
                });

                //mouse out (rollout)
                $(settings.navPreviousWrapper).mouseout(function () {
                    settings.scrollOn = false;
                });

                $(settings.navNextWrapper).mouseout(function () {
                    settings.scrollOn = false;
                });
            }
        }
        return this;
    };

    //slide the list to the left
    var slideListPrevious = function (settings) {
        if (!settings.locked && settings.scrollOn && ((parseInt(settings.listMarginLeft) + parseInt(settings.itemOffset) <= 0) || settings.infiniteScroll == 1)) {
            settings.locked = true;

            if (settings.infiniteScroll == 1) {
                updateListFromBeginning(settings, function () {
                    slideListPreviousAction(settings);
                });
            } else {
                slideListPreviousAction(settings);
            }
        }
    };

    var slideListPreviousAction = function (settings) {
        var offsetUpdate = parseInt(settings.listMarginLeft) + parseInt(settings.itemOffset);

        /* DEBUG */
        if (settings.debug == 1) {
            console.log('offsetUpdate: ' + offsetUpdate);
        }

        $(settings.listWrapper).animate({
            marginLeft: offsetUpdate
        }, settings.slideSpeed, settings.easing, function () {
            if (settings.infiniteScroll == 0) {
                updateListMargin(settings);
            }
            settings.locked = false;

            if (settings.scrollOn) {
                settings.easing = "linear";
                slideListPrevious(settings);
            }
        });
    };

    //slide the list to the right
    var slideListNext = function (settings) {
        if (!settings.locked && settings.scrollOn && (((parseInt(settings.listMarginLeft) - parseInt(settings.itemOffset)) >= settings.itemOffsetMax) || settings.infiniteScroll == 1)) {
            settings.locked = true;

            var offsetUpdate = -parseInt(settings.itemOffset);

            /* DEBUG */
            if (settings.debug == 1) {
                console.log('offsetUpdate: ' + offsetUpdate);
            }

            $(settings.listWrapper).animate({
                marginLeft: offsetUpdate
            }, settings.slideSpeed, settings.easing, function () {
                if (settings.infiniteScroll == 0) {
                    updateListMargin(settings);
                } else {
                    updateListFromEnd(settings, null);
                }
                settings.locked = false;

                if (settings.scrollOn) {
                    settings.easing = "linear";
                    slideListNext(settings);
                }
            });
        }
    };

    var initListItems = function (settings) {
        for (var i = 0; i < settings.totalItem; i++) {
            var pos = parseInt($(settings.listWrapper).find('li').length - 1) - parseInt(i);
            $(settings.listWrapper).find('li:eq(' + pos + ')').clone().insertBefore($(settings.listWrapper).find('li:first-child')).addClass('thslide_copy');
        }
        resetListMargin(settings);
    };

    var updateListFromBeginning = function (settings, callback) {
        var itemToMove = $(settings.listWrapper).find('li:last-child');
        $(settings.listWrapper).find('li:first-child').before(itemToMove);
        $(settings.listWrapper).css('marginLeft', -parseInt(settings.itemOffset));
        updateListMargin(settings);

        if (typeof callback == 'function') {
            return callback();
        }
    };

    var updateListFromEnd = function (settings, callback) {
        var itemToMove = $(settings.listWrapper).find('li:first-child');
        $(settings.listWrapper).find('li:last-child').after(itemToMove);
        $(settings.listWrapper).css('marginLeft', 0);
        updateListMargin(settings);

        if (typeof callback == 'function') {
            return callback();
        }
    };

    //update the left margin of the items list for scrolling effect
    var updateListMargin = function (settings) {
        settings.listMarginLeft = $(settings.listWrapper).css('marginLeft').split('px')[0];
    };

    //reset the left margin of the items list for infinite scrolling
    var resetListMargin = function (settings) {
        //$(settings.listWrapper).css('marginLeft', - parseInt(settings.itemOffset) * settings.totalItem);
        $(settings.listWrapper).css('marginLeft', -parseInt(settings.itemOffset));
        updateListMargin(settings);
    };

})(jQuery);