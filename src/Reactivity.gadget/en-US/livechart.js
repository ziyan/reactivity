var LiveChartSource = function($timespan, $width, $height, $graphDocked, $graphUndocked) {
	this.timespan = $timespan;
	this.delay = 0;
	this.max = 0;
	this.min = 0;
	this.width = $width;
	this.height = $height;
	this.graphDocked = $graphDocked;
	this.graphUndocked = $graphUndocked;
	this.xs = new Array();
	this.ys = new Array();
};
LiveChartSource.prototype.getMax = function() {return this.max;};
LiveChartSource.prototype.getMin = function() {return this.min;};
LiveChartSource.prototype.getDelay = function() {return this.delay;};
LiveChartSource.prototype.getXs = function() {return this.xs;};
LiveChartSource.prototype.getYs = function() {return this.ys;};
LiveChartSource.prototype.getWidth = function() {return this.width;};
LiveChartSource.prototype.getHeight = function() {return this.height;};
LiveChartSource.prototype.getGraphDocked = function() {return this.graphDocked;};
LiveChartSource.prototype.getGraphUndocked = function() {return this.graphUndocked;};
LiveChartSource.prototype.clear = function() {
	this.graphDocked.path = "m 0,0 e";
	this.graphUndocked.path = "m 0,0 e";
	this.xs = new Array();
	this.ys = new Array();
	this.delay = 0;
	this.max = 0;
	this.min = 0;
};
LiveChartSource.prototype.push = function($timestamp, $value) {
	if(this.xs.length > 0 && this.xs[0].getTime() >= $timestamp.getTime()) return;
	var $now = new Date();
	if(this.xs.length<=0)
		this.delay = $now.getTime() - $timestamp.getTime();
	else
		this.delay = parseInt(this.delay * 0.8 + ($now.getTime() - $timestamp.getTime()) * 0.2);
	$now.setTime($now.getTime() - this.delay);
	var $new_xs = new Array();
	var $new_ys = new Array();
	this.min = $value;
	this.max = $value;
	for(var $i=0;$i<this.xs.length;$i++) {
		if($now.getTime() - this.xs[$i].getTime() > this.timespan) break;
		$new_xs[$i+1] = this.xs[$i];
		$new_ys[$i+1] = this.ys[$i];
		if($new_ys[$i+1] > this.max)
			this.max = $new_ys[$i+1];
		else if($new_ys[$i+1] < this.min)
			this.min = $new_ys[$i+1];
	}
	$new_xs[0] = $timestamp;
	$new_ys[0] = $value;
	this.xs = $new_xs;
	this.ys = $new_ys;
};


var LiveChart = function($timespan) {
	this.timespan = $timespan;
	this.sources = new Array();
	this.display = 0.9;
};
LiveChart.prototype.addSource = function($source) {this.sources.push($source);};
LiveChart.prototype.redraw = function() {
	if(this.sources.length<=0) return;
	var $max = 0;
	var $min = 0;
	var $delay = 0;
	var $count = 0;
	//calculate ranges and delays
	for(var $i=0;$i<this.sources.length;$i++) {
		if(this.sources[$i].getXs().length<2) continue;
		if($count == 0 || this.sources[$i].getMax()>$max)
			$max = this.sources[$i].getMax();
		if($count == 0 || this.sources[$i].getMin()<$min)
			$min = this.sources[$i].getMin();
		$delay += this.sources[$i].getDelay();
		$count++;
	}
	if($count<=0) return;
	$delay = $delay / $count;
	var $now = new Date();
	$now.setTime($now.getTime() - $delay);
	var $range = $max - $min;
	if (!$range) $range = 1;
	for(var $j=0;$j<this.sources.length;$j++) {
		if(this.sources[$j].getXs().length<2) continue;
	
		var $xs = this.sources[$j].getXs();
		var $ys = this.sources[$j].getYs();
		var $width = this.sources[$j].getWidth();
		var $height = this.sources[$j].getHeight();
		var $displayHeight = $height * this.display;
		var $displayMargin = ($height - $displayHeight) / 2;
	
		var $path = " m "+parseInt($width)+","+parseInt($displayHeight - (($ys[0] - $min) / $range) * $displayHeight + $displayMargin)+" ";
		var $overflow = false;
		for(var $i=0;$i<$xs.length;$i++) {
			var $x = parseInt($width - ((($now.getTime() - $xs[$i].getTime()) / this.timespan) * $width));
			if($x < 0)
				if(!$overflow) {
					$x = 0;
					$overflow = true;
				} else
					break;
			if($x > $width) $x = $width;
			$path += " l "+$x+","+parseInt($displayHeight - (($ys[$i] - $min) / $range) * $displayHeight + $displayMargin)+" ";
		}
		if(System.Gadget.docked)
			this.sources[$j].getGraphDocked().path = $path + " e";
		else
			this.sources[$j].getGraphUndocked().path = $path + " e";
	}
};
LiveChart.prototype.clear = function() {
	for(var i=0;i<this.sources.length;i++)
		this.sources[i].clear()
	this.sources = new Array();
};

var LiveChartUpdater = new function() {
	this.active = null;
	this.rate = 100;
	this.setRate = function($rate) {
		this.rate = $rate;
	};
	this.begin = function($livechart) {
		this.active = $livechart;
		this.update();
	};
	this.end =  function() {
		if(this.active)
			this.active.clear();
		this.active = null;
	};
	this.update = function() {
		if(this.active==null) return;
			this.active.redraw();
		var $obj = this;
		setTimeout(function(){$obj.update()}, this.rate);
	};
};
