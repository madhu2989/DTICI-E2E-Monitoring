import * as d3 from 'd3';

interface Chart {
  (): any;
  view: Function;
  maxDisplayDatasets: Function;
  curDisplayFirstDataset: Function;
  displayDateRange: Function;
  emphasizeYearTicks: Function;
}

function visavailChart() {

  // define chart layout
  const margin = {
    // top margin includes title and legend
    top: 70,

    // right margin should provide space for last horz. axis title
    right: 0,

    bottom: 20,
    // left margin should provide space for y axis titles
    left: 10,
  };

  // define if tooltip is constant
  let showTooltip: boolean;

  // height of horizontal data bars
  let dataHeight = 18;

  // spacing between horizontal data bars
  const lineSpacing = 14;

  // vertical space for heading
  let paddingTopHeading = -50;

  // vertical overhang of vertical grid lines on bottom
  const paddingBottom = 10;

  // space for y axis titles
  const paddingLeft = 0;

  let width = 2000 - margin.left - margin.right;

  // year ticks to be emphasized or not (default: yes)
  let emphasizeYearTicks = 1;

  // define chart pagination
  // max. no. of datasets that is displayed, 0: all (default: all)
  let maxDisplayDatasets = 0;

  // dataset that is displayed first in the current
  // display, chart will show datasets "curDisplayFirstDataset" to
  // "curDisplayFirstDataset+maxDisplayDatasets"
  let curDisplayFirstDataset = 0;

  // range of dates that will be shown
  // if from-date (1st element) or to-date (2nd element) is zero,
  // it will be determined according to your data (default: automatically)
  let displayDateRange = [0, 0];

  // global div for tooltip
  const div = d3.select('body').append('div')
      .attr('class', 'tooltip')
      .style('opacity', 0);

  let definedBlocks = null;
  let customCategories = null;
  let isDateOnlyFormat = null;

  const chart = <Chart> function (selection) {
    selection.each(function drawGraph(dataset) {
      if (dataset[0].simpleVersion) {
        margin.top = 0;
        if (dataset[0].showTooltip) {
          margin.bottom = 20;
        } else {
          margin.bottom = 0;
        }
        margin.left = 0;

      }
      // check which subset of datasets have to be displayed
      let maxPages = 0;
      let startSet;
      let endSet;
      if (maxDisplayDatasets !== 0) {
        startSet = curDisplayFirstDataset;
        if (curDisplayFirstDataset + maxDisplayDatasets > dataset.length) {
          endSet = dataset.length;
        } else {
          endSet = curDisplayFirstDataset + maxDisplayDatasets;
        }
        maxPages = Math.ceil(dataset.length / maxDisplayDatasets);
      } else {
        startSet = 0;
        endSet = dataset.length;
      }

      // append data attribute in HTML for pagination interface
      selection.attr('data-max-pages', maxPages);

      const noOfDatasets = endSet - startSet;
      const height = dataHeight * noOfDatasets + lineSpacing * noOfDatasets - 1;

      // check how data is arranged
      if (definedBlocks === null) {
        definedBlocks = 0;
        for (let i = 0; i < dataset.length; i++) {
          if (dataset[i].data[0].length === 3) {
            definedBlocks = 1;
            break;
          } else {
            if (definedBlocks) {
              throw new Error('Detected different data formats in input data. Format can either be ' +
                  'continuous data format or time gap data format but not both.');
            }
          }
        }
      }

      // check if data has custom categories
      if (customCategories === null) {
        customCategories = 0;
        for (let i = 0; i < dataset.length; i++) {
          if (dataset[i].data[0][1] !== 0 && dataset[i].data[0][1] !== 1) {
            customCategories = 1;
            break;
          }
        }
      }

      // parse data text strings to JavaScript date stamps
      const parseDateTime = d3.timeFormat('%a %H:%M');
      const parseFullTime = d3.timeFormat('%a %d.%m.%Y %H:%M');
      const strictIsoParse = d3.utcParse("%Y-%m-%dT%H:%M:%S.%LZ");
      const parseDateRegEx = new RegExp(/^\d{4}-\d{2}-\d{2}$/);
      const parseDateTimeRegEx = new RegExp(/^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}.\d{3}$/);
      if (isDateOnlyFormat === null) {
        isDateOnlyFormat = true;
      }

      // cluster data by dates to form time blocks
      dataset.forEach(function (series, seriesI) {
        const tmpData = [];
        const dataLength = series.data.length;
        series.data.forEach(function (d, i) {
            tmpData.push(d);
        });
        dataset[seriesI].disp_data = tmpData;
      });

      // determine start and end dates among all nested datasets
      let startDate = displayDateRange[0];
      let endDate = displayDateRange[1];

      dataset.forEach(function (series, seriesI) {
        if (series.disp_data.length > 0) {
          if (startDate === 0) {
            startDate = series.disp_data[0][0];
            endDate = series.disp_data[series.disp_data.length - 1][2];
          } else {
            if (displayDateRange[0] === 0 && series.disp_data[0][0] < startDate) {
              startDate = series.disp_data[0][0];
            }
            if (displayDateRange[1] === 0 && series.disp_data[series.disp_data.length - 1][2] > endDate) {
              endDate = series.disp_data[series.disp_data.length - 1][2];
            }
          }
        }
      });

      // define scales
      const xScale = d3.scaleLinear()
          .domain([startDate, endDate])
          .range([0, width])
          .clamp(1);

      // create SVG element
      const svg = d3.select(this).append('svg')
          .attr('width', width + margin.left + margin.right)
          .attr('height', height + margin.top + margin.bottom)
          .attr('viewbox', '0 0 ' + (width + margin.left + margin.right) + ' ' + (height + margin.top + margin.bottom))
          .append('g')
          .attr('transform', 'translate(' + margin.left + ',' + margin.top + ')')
          .attr('perserveAspectRatio', 'xMinYMid');

      // create basic element groups
      svg.append('g').attr('id', 'g_title');
      svg.append('g').attr('id', 'g_axis');
      svg.append('g').attr('id', 'g_data');

      // create y axis labels
      const labels = svg.select('#g_axis').selectAll('text')
          .data(dataset.slice(startSet, endSet))
          .enter();

      // text labels
      labels.append('text')
        .attr('x', paddingLeft)
        .attr('y', lineSpacing + dataHeight / 2)
        .text(function (d) {
          if (!(d.measure_html != null)) {
            return d.measure;
          }
        })
        .attr('transform', function (d, i) {
          return 'translate(0,' + ((lineSpacing + dataHeight) * i) + ')';
        })
        .attr('class', function(d) {
          let returnCSSClass = 'ytitle';
          if (d.measure_url != null) {
            returnCSSClass = returnCSSClass + ' link';
          }
          return returnCSSClass;
        })
        .on('click', function(d) {
          if (d.measure_url != null) {
            return window.open(d.measure_url);
          }
          return null;
        });

      // HTML labels
      labels.append('foreignObject')
        .attr('x', paddingLeft)
        .attr('y', lineSpacing)
        .attr('transform', function (d, i) {
          return 'translate(0,' + ((lineSpacing + dataHeight) * i) + ')';
        })
        .attr('width', -1 * paddingLeft)
        .attr('height', dataHeight)
        .attr('class', 'ytitle')
        .html(function(d) {
          if (d.measure_html != null) {
            return d.measure_html;
          }
        });

      // set the ranges
      const x = d3.scaleTime().range([0, width]);
      const y = d3.scaleLinear().range([height, 0]);


      const data = dataset[0].data;

      // format the data
      data.forEach(function(d) {
        d.date = parseDateTime(d.date);
        d.close = +d.close;
      });

      // Scale the range of the data
      x.domain(d3.extent(data, function(d) { return d.date; }));
      y.domain([0, d3.max(data, function(d) { return d.close; })]);

      if (!dataset[0].simpleVersion) {
        // add the X gridlines
        svg.append("g")
        .attr("class", "axis grid")
        .attr("transform", "translate(0," + (height) + ")")
        .call(d3.axisTop(xScale)
          .tickSize(height)
          .tickFormat(parseDateTime)
        );

        // create subtitle
        let subtitleText = '';
        if (noOfDatasets) {
          if (isDateOnlyFormat) {
            subtitleText = 'from ' + parseFullTime(startDate) + ' to '
                + parseFullTime(endDate);
          }
        }

        svg.select('#g_title')
          .append('text')
          .attr('x', paddingLeft)
          .attr('y', paddingTopHeading + 14)
          .text(subtitleText)
          .attr('class', 'subheading');

      }

      // make y groups for different data series
      const g = svg.select('#g_data').selectAll('.g_data')
        .data(dataset.slice(startSet, endSet))
        .enter()
        .append('g')
        .attr('transform', function (d, i) {
          return 'translate(0,' + ((lineSpacing + dataHeight) * i) + ')';
        })
        .attr('class', 'dataset');

      // add data series
      g.selectAll('rect')
        .data(function (d) {
          return d.disp_data;
        })
        .enter()
        .append('rect')
        .attr('x', function (d) {
          return xScale(d[0]);
        })
        .attr('y', lineSpacing)
        .attr('width', function (d) {
          return (xScale(d[2]) - xScale(d[0]));
        })
        .attr('height', dataHeight)
        .attr('class', function (d) {
          if (customCategories) {
            const series = dataset.filter(
              function(serie) {
                return serie.disp_data.indexOf(d) >= 0;
              }
            )[0];
            if (series && series.categories) {
              d3.select(this)
                .attr('fill', series.categories[d[1]].color)
                .attr('stroke-width', 0.1)
                .attr('stroke', 'rgb(0,0,0)');
              return '';
            }
          } else {
            if (d[1] === 1) {
              // data available
              return 'rect_has_data';
            } else {
              // no data available
              return 'rect_has_no_data';
            }
          }
        })
        // TODO Tooltip
        .on('mouseover', function (d, i) {
          const matrix = this.getScreenCTM().translate(+this.getAttribute('x'), +this.getAttribute('y'));
          div.transition()
              .duration(200)
              .style('opacity', 0.9);
          div.html(function () {
            let output = '';
            if (customCategories) {
              // custom categories: display category name
              output = '&nbsp;' + d[3] + ':&nbsp;';
            } else {
              if (d[1] === 1) {
                // checkmark icon
                output = '<i class="fa fa-fw fa-check tooltip_has_data"></i>';
              } else {
                // cross icon
                output = '<i class="fa fa-fw fa-times tooltip_has_no_data"></i>';
              }
            }
            if (isDateOnlyFormat) {
              if (d[2] > d3.timeSecond.offset(d[0], 24 * 60 * 60)) {
                return output + parseDateTime(d[0])
                    + ' - ' + parseDateTime(d[2]);
              }
              return output + parseDateTime(d[0]);
            } else {
              if (d[2] > d3.timeSecond.offset(d[0], 24 * 60 * 60)) {
                return output + parseDateTime(d[0]) + ' '
                    + parseDateTime(d[0]) + ' - '
                    + parseDateTime(d[2]) + ' '
                    + parseDateTime(d[2]);
              }
              return output + parseDateTime(d[0]) + ' - '
                  + parseDateTime(d[2]);
            }
          })
          .style('left', function () {
            return window.pageXOffset + matrix.e + 'px';
          })
          .style('top', function () {
            return window.pageYOffset + matrix.f + 'px';
          })
          .style('z-index', 50, )
          .style('height', 10 + 'px')
          .style('padding-top', dataHeight + 5 + 'px');
        })
        .on('mouseout', function () {
          div.transition()
            .duration(500)
            .style('opacity', 0);
        });
    });
  };

  chart.view = function (_) {
    if (!arguments.length) { return width; }
    width = _[0];
    dataHeight = _[1];
    if (width <= 1000) {
      paddingTopHeading = -75;
    }
    return chart;
  };

  chart.maxDisplayDatasets = function (_) {
    if (!arguments.length) { return maxDisplayDatasets; }
    maxDisplayDatasets = _;
    return chart;
  };

  chart.curDisplayFirstDataset = function (_) {
    if (!arguments.length) { return curDisplayFirstDataset; }
    curDisplayFirstDataset = _;
    return chart;
  };

  chart.displayDateRange = function (_) {
    if (!arguments.length) { return displayDateRange; }
    displayDateRange = _;
    return chart;
  };

  chart.emphasizeYearTicks = function (_) {
    if (!arguments.length) { return emphasizeYearTicks; }
    emphasizeYearTicks = _;
    return chart;
  };

  return chart;
}

export { visavailChart };