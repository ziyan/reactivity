System.Gadget.settingsUI = "settings.html";

var Gadget = new function() {
this.livechart = new LiveChart(60000);
this.statschart = new StatisticsChart();
this.statstimer = null;
this.device = null;
this.onLoad = function() {
	this.refresh();
};
this.onUnload = function() {
	Reactivity.close();
};
this.refresh = function() {
	LiveChartUpdater.end();
	Reactivity.unsubscribe();
	this.statschart.clear();
	if(this.statstimer) {
		clearTimeout(this.statstimer);
		this.statstimer = null;
	}
	this.device = null;
	$("config").style.display="none";
	$("graph").style.display="none";
	$("device").style.display="none";
	$("progress").style.display="none";
	$("control_ac").style.display="none";
	$("control_computer").style.display="none";
	$("status").innerHTML = "";
	if(!System.Gadget.Settings.readString("device") ||
		!System.Gadget.Settings.readString("type")) {
		$("config").style.display="block";
		return;
	}
	$("progress").style.display="block";
	var obj = this;
	Reactivity.userCheckStatus(function(status){obj.userCallback(status);});
};
this.onShowSettings = function() {
	LiveChartUpdater.end();
	Reactivity.unsubscribe();
	Reactivity.eventLoopStop();
	if(this.statstimer) {
		clearTimeout(this.statstimer);
		this.statstimer = null;
	}
};
this.userCallback = function(status) {
	if(status=="OK") {
		Reactivity.eventLoop();
		this.update();
		return;
	}
	$("config").style.display="block";
	$("progress").style.display="none";
};
this.update = function() {
	$("config").style.display="none";
	$("graph").style.display="none";
	$("device").style.display="none";
	$("progress").style.display="block";
	$("control_ac").style.display="none";
	$("control_computer").style.display="none";
	var obj = this;
	if(!Reactivity.deviceList(function(devices){obj.updated(devices);})) {
		$("config").style.display="block";
		$("progress").style.display="none";
	}
};
this.updated = function(devices) {
	if(!devices) {
		$("config").style.display="block";
		$("progress").style.display="none";
		return;
	}
	var device = null;
	for(var i=0;i<devices.length;i++)
		if(devices[i].getGuid() == System.Gadget.Settings.readString("device")) {
			device = devices[i];
			break;
		}
	if(!device) {
		$("config").style.display="block";
		$("progress").style.display="none";
		return;
	}
	this.device = device;
	if(device.getStatus()=="Online") {
		$("device_icon").src = Util.DeviceType.getOnlineIcon(device.getType());
		$("device_data").innerHTML = "Online";
		if(device.getType()==Util.DeviceType.ACNode)
			$("control_ac").style.display="block";
		else if(device.getType()==Util.DeviceType.ComputerNode)
			$("control_computer").style.display="block";
	} else {
		$("device_icon").src = Util.DeviceType.getOfflineIcon(device.getType());
		$("device_data").innerHTML = "Offline";
	}
	$("device_icon").setAttribute("title", Util.DeviceType.getName(device.getType()));
	$("device_name").innerHTML = device.getName();
	$("device_name").setAttribute("title", device.getDescription());
	$("device_description").innerHTML = device.getDescription();
	$("device_type").innerHTML = Util.DeviceType.getName(device.getType());
	$("progress").style.display="none";
	$("device").style.display="block";
	$("graph").style.display="block";
	
	if(System.Gadget.Settings.readString("type") == "live") {
		this.subscribe(device);
		$("status").innerHTML = "Live Data";
	} else if(System.Gadget.Settings.readString("type") == "15m") {
		this.showStatistics(device, 900000, Util.StatisticsType.Minutely, 20000);
		$("status").innerHTML = "15 Minutes Trend";
	} else if(System.Gadget.Settings.readString("type") == "30m") {
		this.showStatistics(device, 1800000, Util.StatisticsType.Minutely, 20000);
		$("status").innerHTML = "30 Minutes Trend";
	} else if(System.Gadget.Settings.readString("type") == "1h") {
		this.showStatistics(device, 3600000, Util.StatisticsType.Minutely, 20000);
		$("status").innerHTML = "1 Hour Trend";
	} else if(System.Gadget.Settings.readString("type") == "6h") {
		this.showStatistics(device, 21600000, Util.StatisticsType.Minutely, 20000);
		$("status").innerHTML = "6 Hours Trend";
	} else if(System.Gadget.Settings.readString("type") == "12h") {
		this.showStatistics(device, 43200000, Util.StatisticsType.Minutely, 20000);
		$("status").innerHTML = "12 Hours Trend";
	} else if(System.Gadget.Settings.readString("type") == "1d") {
		this.showStatistics(device, 86400000, Util.StatisticsType.Hourly, 120000);
		$("status").innerHTML = "1 Day Trend";
	} else if(System.Gadget.Settings.readString("type") == "3d") {
		this.showStatistics(device, 259200000, Util.StatisticsType.Hourly, 1200000);
		$("status").innerHTML = "3 Days Trend";
	} else if(System.Gadget.Settings.readString("type") == "1w") {
		this.showStatistics(device, 604800000, Util.StatisticsType.Hourly, 1200000);
		$("status").innerHTML = "1 Week Trend";
	} else if(System.Gadget.Settings.readString("type") == "2w") {
		this.showStatistics(device, 1209600000, Util.StatisticsType.Daily, 1200000);
		$("status").innerHTML = "2 Weeks Trend";
	} else if(System.Gadget.Settings.readString("type") == "1m") {
		this.showStatistics(device, 2678400000, Util.StatisticsType.Daily, 1200000);
		$("status").innerHTML = "1 Month Trend";
	} else
		$("device_data").innerHTML = "Error";
};
this.showStatistics = function(device, timespan, type, interval) {
	var obj = this;
	var end = new Date();
	var start = new Date();
	start.setTime(start.getTime()-timespan);
	this.statschart.clear();
	this.graph1 = new StatisticsChartSource(start,end,30000,20000,$("graph_docked_1"),$("graph_undocked_1"));
	this.graph2 = new StatisticsChartSource(start,end,30000,20000,$("graph_docked_2"),$("graph_undocked_2"));
	this.graph3 = new StatisticsChartSource(start,end,30000,20000,$("graph_docked_3"),$("graph_undocked_3"));
	this.graph4 = new StatisticsChartSource(start,end,30000,20000,$("graph_docked_4"),$("graph_undocked_4"));
	this.statschart.addSource(this.graph1);
	this.statschart.addSource(this.graph2);
	this.statschart.addSource(this.graph3);
	this.statschart.addSource(this.graph4);
	if(!Reactivity.statisticsQuery(device.getGuid(), -1, type, start, end, function(stats){obj.showStatisticsCallback(stats);}))
		$("device_data").innerHTML = "Error";
	this.statstimer = setTimeout(function(){obj.showStatistics(device,timespan,type,interval);},interval);
};
this.showStatisticsCallback = function(stats) {
	if(!stats) {
		$("device_data").innerHTML = "Error";
		return;
	}
	if(stats.length<=0) {
		$("device_data").innerHTML = "No Data";
		return;
	}
	for(var i=0;i<stats.length;i++) {
		if(stats[i].getService() == Util.ServiceType.Default)
			this.graph1.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.ComputerNode_Memory)
			this.graph2.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.AccelerationSensor_X)
			this.graph2.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.AccelerationSensor_Y)
			this.graph3.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.AccelerationSensor_Z)
			this.graph4.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.ACNode_Current)
			this.graph2.push(stats[i]);
		else if(stats[i].getService() == Util.ServiceType.ACNode_Voltage)
			this.graph3.push(stats[i]);
	}
	this.statschart.redraw();
};
this.subscribe = function(device) {
	if(!Reactivity.currentSubscription() && device.getStatus()=="Online") {
		var obj = this;
		if(!Reactivity.subscribe(device.getGuid(), function(subscription){obj.subscribeCallback(subscription);}))
			$("device_data").innerHTML = "Error";
	}
};
this.subscribeCallback = function(subscription) {
	if(!subscription) {
		$("device_data").innerHTML = "Error";
		return;
	}
	this.livechart.clear();
	this.graph1 = new LiveChartSource(61000,30000,20000,$("graph_docked_1"),$("graph_undocked_1"));
	this.graph2 = new LiveChartSource(61000,30000,20000,$("graph_docked_2"),$("graph_undocked_2"));
	this.graph3 = new LiveChartSource(61000,30000,20000,$("graph_docked_3"),$("graph_undocked_3"));
	this.graph4 = new LiveChartSource(61000,30000,20000,$("graph_docked_4"),$("graph_undocked_4"));
	this.livechart.addSource(this.graph1);
	this.livechart.addSource(this.graph2);
	this.livechart.addSource(this.graph3);
	this.livechart.addSource(this.graph4);
	LiveChartUpdater.begin(this.livechart);
};
this.subscriptionUpdated = function(subscription) {
	if(!subscription || subscription.getStatus()!="Running") {
		LiveChartUpdater.end();
		$("device_data").innerHTML = "Stopped";
	}
};
this.subscriptionNotification = function(subscription,timestamp,service,type,value) {
	if(service == Util.ServiceType.Default) {
		$("device_data").innerHTML = value;
		this.graph1.push(timestamp,value);
	}
	else if(service == Util.ServiceType.ComputerNode_Memory)
		this.graph2.push(timestamp,value);
	else if(service == Util.ServiceType.AccelerationSensor_X)
		this.graph2.push(timestamp,value);
	else if(service == Util.ServiceType.AccelerationSensor_Y)
		this.graph3.push(timestamp,value);
	else if(service == Util.ServiceType.AccelerationSensor_Z)
		this.graph4.push(timestamp,value);
	else if(service == Util.ServiceType.ACNode_Current)
		this.graph2.push(timestamp,value);
	else if(service == Util.ServiceType.ACNode_Voltage)
		this.graph3.push(timestamp,value);

};
this.onDock = function()
{
	document.body.style.height = "99px";
	document.body.style.width = "130px";
	$("bg").style.height = "99px";
	$("bg").style.width = "130px";
	$("bg").src = "url(images/docked.png)";
	$("graph_docked").style.display="block";
	$("graph_undocked").style.display="none";
	$("undocked").disabled=true;
	$("docked").disabled=false;
};
this.onUndock = function()
{
	document.body.style.height = "232px";
	document.body.style.width = "296px";
	$("bg").style.height = "232px";
	$("bg").style.width = "296px";
	$("bg").src = "url(images/undocked.png)";
	$("graph_docked").style.display="none";
	$("graph_undocked").style.display="block";
	$("undocked").disabled=false;
	$("docked").disabled=true;
};
this.onSettingsClosed = function(e) {
	this.refresh();
};
this.visibilityChanged = function(e) {
	if(System.Gadget.docked)
		this.onDock();
	else
		this.onUndock();
};
this.onControlComputer = function() {
	var value = $("control_computer").value;
	$("control_computer").value = "";
	if(!value) return;
	if(!this.device) return;
	if(this.device.getType()!=Util.DeviceType.ComputerNode) return;
	if(this.device.getStatus()!="Online") return;
	Reactivity.dataSend(this.device.getGuid(), Util.ServiceType.Default, Util.DataType.Short, value, null);
};
this.onControlAC = function() {
	var value = $("control_ac").value;
	$("control_ac").value = "";
	if(!value) return;
	if(!this.device) return;
	if(this.device.getType()!=Util.DeviceType.ACNode) return;
	if(this.device.getStatus()!="Online") return;
	Reactivity.dataSend(this.device.getGuid(), Util.ServiceType.ACNode_Relay, Util.DataType.Bool, value=="on"?"true":"false", null);
};
};
System.Gadget.onDock = function(){Gadget.onDock()};
System.Gadget.onUndock = function(){Gadget.onUndock()};
System.Gadget.onShowSettings = function(){Gadget.onShowSettings();};
System.Gadget.onSettingsClosed = function(e){Gadget.onSettingsClosed(e);};
System.Gadget.visibilityChanged = function(e){Gadget.visibilityChanged(e);};

