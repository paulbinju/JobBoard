// JavaScript Document


	jQuery(document).ready(function($) {
	 var owl = $('.testimoneal-carousel');
      owl.owlCarousel({
        margin: 10,
        loop: true,
		navigation: true,
		pagination: true,
		nav:true,
		autoPlay: true,
        responsive: {
          0: {
            items: 1
          },
		   639: {
            items: 2
          },
		   
		   900: {
            items: 3
          },
          1200: {
            items: 3
          },
          
		  
        }
      })
	  
	});



	jQuery(document).ready(function($) {
	 var owl = $('.client-carouselpartner');
      owl.owlCarousel({
        margin: 10,
        loop: true,
		navigation: false,
		pagination: false,
		autoPlay: true,
        responsive: {
          0: {
            items: 2
          },
		   400: {
            items: 3
          },
		   670: {
            items: 4
          },
		   900: {
            items: 4
          },
          1199: {
            items: 4
          },
          1200: {
            items: 5
          }
        }
      })
	});
	
	
	jQuery(document).ready(function($) {
	 var owl = $('.topemp-carousel');
      owl.owlCarousel({
        margin: 10,
        loop: true,
		navigation: false,
		pagination: false,
		autoPlay: true,
        responsive: {
          0: {
            items: 2
          },
		   400: {
            items: 2
          },
		   670: {
            items: 3
          },
		   900: {
            items: 4
          },
          1199: {
            items: 5
          },
          1200: {
            items: 6
          }
        }
      })
	});
	
	

 $(document).ready(function() {
	$(".client-carousel").owlCarousel({
		items :5,
		itemsDesktop : [1199,4],
		itemsDesktopSmall : [900,3], // betweem 900px and 601px
		itemsTablet: [800,3], 
		itemsTabletSmall: [670,2], 
		itemsMobile: [400,1],
		navigation: false,
		pagination: false,
		autoPlay: true,
		navigationText: [
				"<img src='images/client_rightarrow.png' alt='' />",
				"<img src='images/client_leftarrow.png' alt='' />"
	   ],
	});
	
//	$(".testimoneal-carousel").owlCarousel({
//		items :3,
//		itemsDesktop : [1199,3],
//		itemsDesktopSmall : [900,2], // betweem 900px and 601px
//		itemsTablet: [800,2], 
//		itemsTabletSmall: [670,2], 
//		itemsMobile: [639,1],
//		navigation: true,
//		pagination: false,
//		autoPlay: true,
		//singleItem : true,
//		navigationText: [
//				"<img src='images/client_rightarrow.png' alt='' />",
//				"<img src='images/client_leftarrow.png' alt='' />"
//	   ],
//	});
	
//	$(".topemp-carousel").owlCarousel({
//		items :6,
//		itemsDesktop : [1199,4],
//		itemsDesktopSmall : [900,3], // betweem 900px and 601px
//		itemsTablet: [800,3], 
//		itemsTabletSmall: [670,2], 
//		itemsMobile: [400,1],
//		navigation: false,
//		pagination: false,
//		autoPlay: true,
//		navigationText: [
//				"<img src='images/client_rightarrow.png' alt='' />",
//				"<img src='images/client_leftarrow.png' alt='' />"
//	   ],
//	});
	
<!------ >	
	
//	$(function () {
//		$(".loadbox").slice(0, 4).show();
//		$("#loadMore").on('click', function (e) {
//			e.preventDefault();
//			$(".loadbox:hidden").slice(0, 4).slideDown();
//			if ($(".loadbox:hidden").length == 0) {
//				$("#load").fadeOut('slow');
//			}
//		});
//	});
	
	
	/*$(function () {
		$(".loadbox").slice(0, 4).show();
		$("#loadMore").on('click', function (e) {
			e.preventDefault();
			$(".loadbox:hidden").slice(0, 4).slideDown();
			if ($(".loadbox:hidden").length == 0) {
				$("#load").fadeOut('slow');
			}
		});
		$('#showless').click(function () {
        x=(x-5<0) ? 4 : x-5;
        $('.loadbox').not(':lt('+x+')').hide();
    });
	});*/
	
	$(document).ready(function () {
		size_li = $(".joblistboxsec .loadbox").size();
		x=4;
		$('.joblistboxsec .loadbox:lt('+x+')').slideDown();
		$('#loadMore').click(function () {
			x= (x+4 <= size_li) ? x+4 : size_li;
			$('.joblistboxsec .loadbox:lt('+x+')').slideDown();
			if(x==size_li){ $('#showLess').show(); $('#loadMore').hide();  }
		});
		$('#showLess').click(function () {
			//x=(x-4<0) ? 4 : x-4;
		
			//if(x<4){ $('#showLess').hide(); $('#loadMore').show(); return false;  }
			//$('.joblistboxsec .loadbox').not(':lt('+x+')').slideUp();
            //if(x<=4){ $('#showLess').hide(); $('#loadMore').show();   }
            
		});
	});
	
	
	$(document).ready(function () {
	  $('.accordion-tabs-minimal').each(function(index) {
		$(this).children('li').first().children('a').addClass('is-active').next().addClass('is-open').show();
	  });
	  $('.accordion-tabs-minimal').on('click', 'li > a.tab-link', function(event) {
		if (!$(this).hasClass('is-active')) {
		  event.preventDefault();
		  var accordionTabs = $(this).closest('.accordion-tabs-minimal');
		  accordionTabs.find('.is-open').removeClass('is-open').hide();
	
		  $(this).next().toggleClass('is-open').toggle();
		  accordionTabs.find('.is-active').removeClass('is-active');
		  $(this).addClass('is-active');
		} else {
		  event.preventDefault();
		}
	  });
	});
	
	
	$( function() {
    $( "#datepicker" ).datepicker({
      changeMonth: true,
      yearRange: "1950:2012",
      changeYear: true
    });
  } );
  
		
	$('.sticky').stickMe(); 
	
	
	
	wow = new WOW(
    {
        animateClass: 'animated',
        offset:       100,
       callback:     function(box) {
        
       }
      }
    );
    wow.init();
			
});




