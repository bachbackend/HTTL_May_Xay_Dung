jQuery(document).ready(function () {

  /* Remove wpadminbar to keep sticky header work correct */
  if ( !isElementorPreviewIframe() && ( window.self !== window.top ) ) {
    jQuery('#wpadminbar').remove();
  }

  /* Remove in Elementor edit page preview iframe */
  if ( isElementorPreviewIframe() ) {
    jQuery(document).find(".tbdemo-sharebar-container,.tbdemo-ai-popup-layout,.tbdemo-ai-popup-container,.tbdemo-customize-container").remove();
  }

  tbdemo_change_domain_id_url();

  jQuery(document).on("click", ".single_add_to_cart_button, .add_to_cart_button, .ajax_add_to_cart", function() {
    if ( typeof dataLayer != "undefined" ) {
      dataLayer.push({
        event: '10web-event',
        'eventCategory': 'AI Builder Demo',
        'eventAction': 'Ecommerce: Add to cart',
        'eventLabel' : '-'
      });
    }
  })

  if ( !isElementorPreviewIframe() && ( window.self !== window.top ) ) {

    tbdemo_fake_editor_marker();

    jQuery(document).find(".tbdemo_eicon").on("click", function () {
      window.parent.postMessage('upgrade', taa.dashboard_url);
    });

    jQuery(document).on('click', 'a[href]:not(.add_to_cart_button)', function (event) {
      let href = jQuery(this).attr('href');

      if (href && href.trim() !== '' && !href.endsWith('#')) {
        window.parent.postMessage('urlChange', taa.dashboard_url);
      }
    });

    /* Code need to send scroll info to frontend */
    let sent50 = false;
    let sent90 = false;

    jQuery(window).on("scroll", function () {
      let scrollTop = jQuery(this).scrollTop();
      let height = jQuery(document).height() - jQuery(this).height();
      if (height === 0) {
        return;
      }
      let scrollPercent = (scrollTop / height * 100);

      if (!sent50 && scrollPercent >= 50) {
        window.parent.postMessage("scroll50", taa.dashboard_url);
        sent50 = true;
      }

      if (!sent90 && scrollPercent >= 90) {
        window.parent.postMessage("scroll90", taa.dashboard_url);
        sent90 = true;
      }
    });

    jQuery(window).on('resize', function () {
      if (jQuery(window).width() <= 700) {
        jQuery(document).find(".tbdemo_eicon").hide();
      } else {
        jQuery(document).find(".tbdemo_eicon").show();
      }
    });
  }

});

function tbdemo_fake_editor_hover() {
  jQuery(document).on('mouseenter', '.elementor-widget, div[data-element_type="container"]', function () {
    if ( jQuery(window).width() <= 700 ||
         jQuery(this).hasClass("elementor-widget-twbb_woocommerce-menu-cart") ||
         jQuery(this).hasClass("elementor-widget-icon") )  return;

    let $this = jQuery(this);

    // Add the correct class if it's a widget
    if ($this.hasClass("elementor-widget")) {
      $this.addClass('tbdemo-widget-outline');
    }

    // Find all ancestor containers
    let parentContainers = $this.parents('div[data-element_type="container"]');

    if (parentContainers.length) {
      // The uppermost container (the farthest up in the hierarchy)
      let upperContainer = parentContainers.last();
      upperContainer.addClass('tbdemo-container-outline');

      // Add column outline to all containers except the uppermost one
      parentContainers.not(upperContainer).addClass('tbdemo-column-outline');

      // If the current element is a child container, mark it as tbdemo-column-outline
      if ($this.is('div[data-element_type="container"]')) {
        $this.addClass('tbdemo-column-outline');
      }

      /* Need to exclude header cart icon fake editor as it is small and edit icon make it not clickable */
      if($this.is('div[data-element_type="container"]') && $this.children('.elementor-widget-twbb_woocommerce-menu-cart').length > 0 ) {
        $this.find('.tbdemo_eicon').remove();
        $this.removeClass('tbdemo-column-outline');
      }
    } else {
      // If this is the uppermost container (no parents), add only the container outline
      $this.addClass('tbdemo-container-outline');
    }

    // Make icons visible
    $this.parents().children(".tbdemo_eicon").css("opacity", 1);
    $this.children(".tbdemo_eicon").css("opacity", 1);
  });

  jQuery(document).on('mouseleave', '.elementor-widget, div[data-element_type="container"]', function (event) {
    if ( jQuery(window).width() <= 700 ) return;
    jQuery(this).removeClass("tbdemo-widget-outline tbdemo-column-outline tbdemo-container-outline");
    jQuery(this).find(".tbdemo_eicon").css("opacity", 0)
  });

  jQuery(document).find(".tbdemo_eicon").hover(
      function () {
        let parent = jQuery(this);
        let tooltip = parent.find(".tbdemo-edit-tooltip");

        if (!tooltip.length) return; // Exit if no tooltip is found

        let parentOffset = parent.offset();
        let tooltipHeight = tooltip.outerHeight();
        let stickyHeader = jQuery("[data-elementor-type='twbb_header'] .elementor-element.elementor-sticky--active");

        let headerBottom = stickyHeader.length ? stickyHeader.offset().top + stickyHeader.outerHeight() : 0;
        let spaceBetweenHeaderAndParent = parentOffset.top - headerBottom; // Actual free space above

        // If there is NOT enough space ABOVE the parent (after considering sticky header), move tooltip below
        if (spaceBetweenHeaderAndParent < tooltipHeight + 10) {
          tooltip.addClass("tbdemo-tooltip-bottom");
        } else {
          tooltip.removeClass("tbdemo-tooltip-bottom");
        }
      },
      function () {
        // Remove the class on hover out if needed
        jQuery(this).find(".tbdemo-edit-tooltip").removeClass("tbdemo-tooltip-bottom");
      }
  );
}

function tbdemo_fake_editor_marker() {
  jQuery(".tbdemo_container_edit_settings, .tbdemo_eicon").remove();
  let container_edit_icons = "<ul class='tbdemo_eicon tbdemo_container_edit_settings'>";
  container_edit_icons += "<li class='tbdemo_container_edit_settings-add' title='Add Container'>";
  container_edit_icons += "<i class='eicon-plus' aria-hidden='true'></i>";
  container_edit_icons += "</li>";
  container_edit_icons += "<li class='tbdemo_container_edit_settings-edit' title='Edit Container'>";
  container_edit_icons += "<i class='eicon-handle' aria-hidden='true'></i>";
  container_edit_icons += "</li>";
  container_edit_icons += "<li class='tbdemo_container_edit_settings-remove' title='Delete Container'>";
  container_edit_icons += "<i class='eicon-close' aria-hidden='true'></i>";
  container_edit_icons += "</li>";
  container_edit_icons += "<span class='tbdemo-edit-tooltip'>Customize with 10Web Pro</span>";
  container_edit_icons += "</ul>";

  jQuery('body div.elementor[data-elementor-type] > [data-element_type="container"]').each(function () {
    jQuery(this).append(container_edit_icons);
    jQuery(this).find("div[data-element_type='container']").append("<i class='tbdemo_eicon tbdemo_eicon-handle eicon-handle'><span class='tbdemo-edit-tooltip'>Customize with 10Web Pro</span></i>");
    let widget = jQuery(this).find("div[data-element_type='widget']");

    widget.each(function () {
      let e = jQuery(this);
      /* Need to exclude header cart icon fake editor as it is small and edit icon make it not clickable */
      if( !e.hasClass("elementor-widget-twbb_woocommerce-menu-cart") && !e.hasClass("elementor-widget-icon") ) {
        e.append("<i class='tbdemo_eicon tbdemo_eicon-edit eicon-edit'><span class='tbdemo-edit-tooltip'>Customize with 10Web Pro</span></i>");
      }
    })
  });

  tbdemo_fake_editor_hover();

  if ( jQuery(window).width() <= 700 ) {
    jQuery(document).find(".tbdemo_eicon").hide();
  } else {
    jQuery(document).find(".tbdemo_eicon").show();
  }

}


function isElementorPreviewIframe() {
  return window.frameElement && window.frameElement.id === 'elementor-preview-iframe';
}

/* The function set value to twbb_sidebar_vars.upgrade_url which used in the builder plugin */
function tbdemo_change_domain_id_url() {
  if( typeof twbb_sidebar_vars === 'undefined' ) return;
  let str = twbb_sidebar_vars.upgrade_url;


  const regex = /websites\/(\d+)\//gm;

  let m;
  let domain_id = tbdemo_getCookie("tbdemo_domain_id");
  if (!domain_id) {
    return;
  }

  let mathch1 = "";
  let mathch2 = "";
  while ((m = regex.exec(str)) !== null) {
    // This is necessary to avoid infinite loops with zero-width matches
    if (m.index === regex.lastIndex) {
      regex.lastIndex++;
    }
    mathch1 = m[0];
    mathch2 = m[1];
  }

  twbb_sidebar_vars.upgrade_url = str.replace(mathch1, mathch1.replace(mathch2, domain_id));
}


function tbdemo_getCookie(cname) {
  let name = cname + "=";
  let decodedCookie = decodeURIComponent(document.cookie);
  let ca = decodedCookie.split(';');
  for(let i = 0; i <ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) == ' ') {
      c = c.substring(1);
    }
    if (c.indexOf(name) == 0) {
      return c.substring(name.length, c.length);
    }
  }
  return "";
}

window.addEventListener("message", (event) => {
  if (event.origin !== taa.dashboard_url) return;
  if (event.data && event.data.type === "color") {
    console.log(event.data.data);
    const obj = new ThemeCustomize();
    obj.active_color = event.data.data;
    obj.changeThemeStyle( obj, 'color' );
  }
  if (event.data && event.data.type === "font") {
    console.log(event.data.data);
    const obj = new ThemeCustomize();
    obj.active_font = event.data.data;
    obj.changeThemeStyle( event.data.data, 'font' );
  }

  if (event.data && event.data.type === "reload") {
    window.location.href = taa.home_permalink;
  }
});
