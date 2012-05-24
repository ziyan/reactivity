var StatisticsChartSource = function($start, $end, $width, $height, $graphDocked, $graphUndocked) {
	this.stats = new Array();
	this.width = $width;
	this.height = $height;
	this.graphDocked = $graphDocked;
	this.graphUndocked = $graphUndocked;
	this.max = 0;
	this.min = 0;
	this.start = $start;
	this.end = $end;
};
StatisticsChartSource.prototype.getMax = function() {return this.max;};
StatisticsChartSource.prototype.getMin = function() {return this.min;};
StatisticsChartSource.prototype.getStart = function() {return this.start;};
StatisticsChartSource.prototype.getEnd = function() {return this.end;};
StatisticsChartSource.prototype.getStats = function() {return this.stats;};
StatisticsChartSource.prototype.getWidth = function() {return this.width;};
StatisticsChartSource.prototype.getHeight = function() {return this.height;};
StatisticsChartSource.prototype.getGraphDocked = function() {return this.graphDocked;};
StatisticsChartSource.prototype.getGraphUndocked = function() {return this.graphUndocked;};
StatisticsChartSource.prototype.push = function($stat) {
	if($stat.getDate().getTime() > this.end.getTime()) return;
	if($stat.getDate().getTime() < this.start.getTime()) return;
	if(this.stats.length == 0)
	{
		this.max = $stat.getValue();
		this.min = $stat.getValue();
		this.stats.push($stat);
		return;
	}
	if(this.max < $stat.getValue())
		this.max = $stat.getValue();
	if(this.min > $stat.getValue())
		this.min = $stat.getValue();
	this.stats.push($stat);
};

StatisticsChartSource.prototype.clear = function() {
	this.graphDocked.path = "m 0,0 e";
	this.graphUndocked.path = "m 0,0 e";
	this.stats=new Array();
	this.max=0;
	this.min=0;
};

var StatisticsChart = function() {
	this.sources = new Array();
	this.display = 0.9;
};
StatisticsChart.prototype.addSource = function($source) {this.sources.push($source);};
StatisticsChart.prototype.redraw = function() {
	if(this.sources.length<=0) return;
	var $max = null;
	var $min = null;
	var $start = null;
	var $end = null;
	//calculate ranges
	for(var $i=0;$i<this.sources.length;$i++) {
		if(this.sources[$i].getStats().length<2) continue;
		if($max == null || this.sources[$i].getMax()>$max)
			$max = this.sources[$i].getMax();
		if($min == null || this.sources[$i].getMin()<$min)
			$min = this.sources[$i].getMin();
		if($start == null || this.sources[$i].getStart().getTime()<$start.getTime())
			$start = this.sources[$i].getStart();
		if($end == null || this.sources[$i].getEnd().getTime()>$end.getTime())
			$end = this.sources[$i].getEnd();
	}
	if(!$start||!$end) return;
	var $range = $max - $min;
	var $timespan = $end.getTime() - $start.getTime();
	if (!$range) $range = 1;
	if (!$timespan) $timespan = 1;
	for(var $j=0;$j<this.sources.length;$j++) {
		if(this.sources[$j].getStats().length<2) continue;
		var $stats = this.sources[$j].getStats();
		var $width = this.sources[$j].getWidth();
		var $height = this.sources[$j].getHeight();
		var $displayHeight = $height * this.display;
		var $displayMargin = ($height - $displayHeight) / 2;
	
		var $path = " m "+parseInt($width)+","+parseInt($displayHeight - (($stats[$stats.length-1].getValue() - $min) / $range) * $displayHeight + $displayMargin)+" ";
		var $overflow = false;
		for(var $i=0;$i<$stats.length;$i++) {
			var $x = parseInt($width - ((($end.getTime() - $stats[$i].getDate().getTime()) / $timespan) * $width));
			if($x < 0)
				if(!$overflow) {
					$x = 0;
					$overflow = true;
				} else
					break;
			if($x > $width) $x = $width;
			$path += " l "+$x+","+parseInt($displayHeight - (($stats[$i].getValue() - $min) / $range) * $displayHeight + $displayMargin)+" ";
		}
		this.sources[$j].getGraphDocked().path = $path + " e";
		this.sources[$j].getGraphUndocked().path = $path + " e";
	}
};
StatisticsChart.prototype.clear = function() {
	for(var i=0;i<this.sources.length;i++)
		this.sources[i].clear()
	this.sources = new Array();
};