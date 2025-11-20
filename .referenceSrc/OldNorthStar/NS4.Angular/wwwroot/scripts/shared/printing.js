if (window.matchMedia) {
    // chrome & safari (ff supports it but doesn't implement it the way we need)
    var mediaQueryList = window.matchMedia("print");

    mediaQueryList.addListener(function (mql) {
        if (mql.matches) {
            reflowForPrinting();
        } else {
            reflowAfterPrinting();
        }
    });
}

window.addEventListener("beforeprint", function (ev) {
    reflowForPrinting();
});

window.addEventListener("afterprint", function (ev) {
    reflowAfterPrinting();
});

function reflowForPrinting() {
    if (typeof Highcharts.charts !== "undefined") {
        console.log("Resizing charts ready for printing", new Date());
        reflowTheseCharts(Highcharts.charts);
    }
}

function reflowAfterPrinting() {
    if (typeof Highcharts.charts !== "undefined") {
        console.log("Resizing charts back to screen size after printing", new Date());
        reflowTheseCharts(Highcharts.charts);
    }
}

function reflowTheseCharts(charts) {
    charts.forEach(function (chart) {
        // I'm assuming this check is quicker to execute than a chart 
        // reflow so this check is actually saving time...
        if (typeof chart !== "undefined") {
            console.log("reflowing chart ");
            //chart.setSize(800, 600);
            chart.reflow();
        }
        
        
    });
}

if (location.href.indexOf('printmode=') >= 0) {
    var styles = document.createElement('link');
    styles.rel = 'stylesheet';
    styles.type = 'text/css';
    styles.media = 'all';
    styles.href = 'styles/nspdfstyles.css';
    document.getElementsByTagName('head')[0].appendChild(styles);
}