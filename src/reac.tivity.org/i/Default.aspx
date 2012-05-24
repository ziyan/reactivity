<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Language" content="en-us" />
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Reactivity</title>
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0"/>
<link rel="apple-touch-icon" href="images/apple-touch-icon.png" />
<style type="text/css" media="screen">@import "styles/reactivity.css";</style>
<script type="text/javascript" language="javascript" src="scripts/prototype.js"></script>
<script type="text/javascript" language="javascript" src="scripts/reactivity.js"></script>
<script type="text/javascript" language="javascript" src="scripts/util.js"></script>
<script type="text/javascript" language="javascript" src="scripts/page.js"></script>
</head>
<body onorientationchange="Pages.orientationChanged()">
<div id="splash"><img src="images/splash.png" alt="Reactivity"/><div id="splash_message"></div></div>
<div id="floorplan"></div>
<h1 id="page_title">Reactivity</h1>
<a id="backButton" class="back" href="#" onclick="Pages.backButtonClicked();return false;">Reactivity</a>
<a id="specialButton" class="special" href="#" onclick="Pages.specialButtonClicked();return false;">Logout</a>
<div id="page_content"></div>
<div id="page_pool" style="display:none;">
<div id="page_home">
<h2><strong>Welcome</strong></h2>
<ul>
<li class="linked" onclick="Pages.go('user');"><img alt="" src="images/icons/user.png" class="icon"/><span id="home_user">User</span></li>
<li class="linked" onclick="Pages.go('buildings');"><img alt="" src="images/icons/buildings.png" class="icon"/>Buildings</li>
<li class="linked" onclick="Pages.go('devices');"><img alt="" src="images/icons/devices.png" class="icon"/>Devices</li>
</ul>
<h2><strong>About Reactivity</strong></h2>
<ul>
<li><center><img alt="logo" src="images/logo.png" /></center></li>
<li class="linked" onclick="Pages.go('about');">About</li>
</ul>
</div>

<div id="page_about">
<h2><strong>About Reactivity</strong></h2>
<ul>
<li><center><img alt="logo" src="images/logo.png" /></center></li>
</ul>
<ul>
<li class="linked" onclick="window.open('http://sparxlive.com')">Visit Team Sparx</li>
<li class="article">Reactivity sensor network provides home, enterprise and city-level networking
of sensor and device control systems. It utilizes Windows Communication Foundation
to coordinate sensor devices placed inside constructions. On one hand, different
kinds of data, like temperature, humidity, luminosity, AC current, sound, carbon
dioxide and monoxide, and motion, are being collected and processed through
Reactivity servers, and reported to users. On the other hand, commands can be
generated and pushed back to devices to control air conditioner, computers, and
AC power. The network can be configured and programmed using customized rules.
Different actions can be scheduled to happen once certain environmental conditions
are met in the targeted environment. Microsoft SQL Server would be used to store
sensor position and configuration information as well as sensor data for future
study and analysis. Mobile phone users would be able to access sensor data as
well as control environment remotely through this ASP.NET powered mobile web.</li>
<li class="article">Copyright &copy; 2008 sparxlive.com</li>
</ul>
</div>

<div id="page_login">
<h2><img alt="" src="images/icons/login.png" class="icon_title"/><strong>User Login</strong></h2>
<div id="login_before">
<ul>
<li><label for="login_uri"><img alt="" src="images/icons/server.png" class="icon"/></label>
<input id="login_uri" name="login_uri" type="text" placeholder="Server" value="http://localhost/client.svc" /></li>
<li><label for="login_username"><img alt="" src="images/icons/user.png" class="icon"/></label>
<input id="login_username" name="login_username" type="text" placeholder="Username"/></li>
<li><label for="login_password"><img alt="" src="images/icons/password.png" class="icon"/></label>
<input id="login_password" name="login_password" type="password" /></li>
<li><img alt="" src="images/icons/remember.png" class="icon"/>Remember Me<div id="login_remember"></div></li>
</ul>
<input id="login_submit" type="submit" style="display:none;" />
<a href="#" id="login_button" onclick="LoginPage.login();return false;" class="button white">Login</a>
</div>
<div id="login_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
</div>


<div id="page_user">
<h2><img alt="" src="images/icons/user.png" class="icon_title"/><strong>User Information</strong></h2>
<ul>
<li><strong>ID: </strong><span id="user_id">0</span></li>
<li><strong>Username: </strong><span id="user_username"></span></li>
<li><strong>Name: </strong><span id="user_name"></span></li>
<li><strong>Description: </strong><span id="user_description"></span></li>
</ul>
<h2><img alt="" src="images/icons/password.png" class="icon_title"/><strong>Change Password</strong></h2>
<div id="user_password_form">
<ul>
<li><label for="user_password_old"><img alt="" src="images/icons/password_old.png" class="icon"/></label>
<input id="user_password_old" name="user_password_old" type="password" /></li>
<li><label for="user_password_new"><img alt="" src="images/icons/password_new.png" class="icon"/></label>
<input id="user_password_new" name="user_password_new" type="password"/></li>
<li><label for="user_password_new2"><img alt="" src="images/icons/password_new.png" class="icon"/></label>
<input id="user_password_new2" name="user_password_new2" type="password" /></li>
</ul>
<input id="user_password_submit" type="submit" style="display:none;" />
<a href="#" id="user_password_button" onclick="UserPage.changePassword(); return false;" class="button white">Change Password</a>
</div>
<div id="user_password_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
</div>

<div id="page_buildings">
<div id="buildings_info">
<h2><img alt="" src="images/icons/buildings.png" class="icon_title"/><strong>Buildings</strong></h2>
<ul id="buildings_list">
</ul>
<h2><img alt="" src="images/icons/devices.png" class="icon_title"/><strong>All Devices</strong></h2>
<ul id="buildings_devices"></ul>
</div>
<div id="buildings_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
</div>

<div id="page_devices">
<h2><img alt="" src="images/icons/devices.png" class="icon_title"/><strong>All Devices</strong></h2>
<ul id="devices_list">
</ul>
</div>

<div id="page_building">
<div id="building_info">
<h2><img alt="" src="images/icons/building.png" class="icon_title"/><strong><span id="building_name"></span></strong></h2>
<ul>
<li id="building_description"></li>
<li onclick="BuildingPage.showOnMap()"><img alt="" src="images/icons/maps.png" class="icon_right"/>Show On Map</li>
</ul>
<h2><img alt="" src="images/icons/floors.png" class="icon_title"/><strong>Floors</strong></h2>
<ul id="building_floors"></ul>
<h2><img alt="" src="images/icons/devices.png" class="icon_title"/><strong>Devices In This Building</strong></h2>
<ul id="building_devices"></ul>
</div>
<div id="building_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
</div>

<div id="page_floor">
<div id="floor_info">
<h2><strong><img alt="" src="images/icons/floor.png" class="icon_title"/><span id="floor_name"></span></strong></h2>
<ul>
<li id="floor_description"></li>
<li onclick="FloorPage.gotoBuilding();"><img alt="" src="images/icons/building.png" class="icon_right"/><strong>Building: </strong><span id="floor_building"></span></li>
<li><strong>Level: </strong><span id="floor_level"></span></li>
<li onclick="FloorPage.gotoFloorplan();"><img alt="" src="images/icons/floorplan.png" class="icon_right"/>Show Floorplan</li>
</ul>
<h2><img alt="" src="images/icons/devices.png" class="icon_title"/><strong>Devices On This Floor</strong></h2>
<ul id="floor_devices"></ul>
</div>
<div id="floor_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
</div>

<div id="page_device">
<div id="device_progress">
<ul><li>Please Wait ...<img alt="loading" class="loading" src="images/loading.gif"/></li></ul>
</div>
<div id="device_info">
<h2><strong><span id="device_name"></span></strong></h2>
<ul>
<li id="device_description"></li>
<li><strong>Type: </strong><span id="device_type"></span></li>
<li><strong>Status: </strong><span id="device_status"></span></li>
</ul>
</div>
<div id="device_location">
<h2><img alt="" src="images/icons/buildings.png" class="icon_title"/><strong>Location</strong></h2>
<ul>
<li onclick="DevicePage.gotoBuilding();"><img alt="" src="images/icons/building.png" class="icon_right"/><strong>Building: </strong><span id="device_building"></span></li>
<li onclick="DevicePage.gotoFloor();"><img alt="" src="images/icons/floor.png" class="icon_right"/><strong>Floor: </strong><span id="device_floor"></span></li>
<li onclick="DevicePage.gotoFloorplan();"><img alt="" src="images/icons/maps.png" class="icon_right"/>Show On Floorplan</li>
</ul>
</div>
<div id="device_chart">
<h2><img alt="" src="images/icons/data.png" class="icon_title"/><strong>Data Feedback</strong></h2>
<ul>
<li id="device_display"><center><canvas id="device_canvas" width="270" height="100" style="overflow: hidden; margin: 10px 0;background:url(images/livechart.png);"></canvas></center></li>
<li id="device_data"></li>
<li><strong>Showing: </strong><select id="device_chart_type" onchange="DevicePage.chartTypeChanged();">
<option value="">None</option>
<optgroup id="device_chart_type_subscribe" label="Subscribe" disabled="disabled">
<option value="live">Live Data</option>
</optgroup>
<optgroup id="device_chart_type_stats" label="Statistics" disabled="disabled">
<option value="15m">15 minutes trend</option>
<option value="30m">30 minutes trend</option>
<option value="1h">1 hour trend</option>
<option value="6h">6 hours trend</option>
<option value="12h">12 hours trend</option>
<option value="1d">1 day trend</option>
<option value="3d">3 days trend</option>
<option value="1w">1 week trend</option>
<option value="2w">2 weeks trend</option>
<option value="1m">1 month trend</option>
</optgroup>
</select></li>
</ul>
</div>
<div id="device_control_computer">
<h2><img alt="" src="images/icons/computer.png" class="icon_title"/><strong>Control This Computer</strong></h2>
<ul>
<li onclick="if(confirm('Are you sure to log off the user on this computer?')) DevicePage.controlComputer(1);"><img alt="" src="images/icons/logoff.png" class="icon_right"/>Log Off</li>
<li onclick="if(confirm('Are you sure to reboot this computer?')) DevicePage.controlComputer(2);"><img alt="" src="images/icons/reboot.png" class="icon_right"/>Reboot</li>
<li onclick="if(confirm('Are you sure to suspend this computer?')) DevicePage.controlComputer(3);"><img alt="" src="images/icons/suspend.png" class="icon_right"/>Suspend</li>
<li onclick="if(confirm('Are you sure to hibernate this computer?')) DevicePage.controlComputer(4);"><img alt="" src="images/icons/hibernate.png" class="icon_right"/>Hibernate</li>
<li onclick="if(confirm('Are you sure to shutdown this computer?')) DevicePage.controlComputer(5);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Shutdown</li>
<li onclick="if(confirm('Are you sure to power off this computer?')) DevicePage.controlComputer(6);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Power Off</li>
</ul>
<ul>
<li onclick="if(confirm('Are you sure to forcely log off the user on this computer?')) DevicePage.controlComputer(-1);"><img alt="" src="images/icons/logoff.png" class="icon_right"/>Log Off (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely reboot this computer?')) DevicePage.controlComputer(-2);"><img alt="" src="images/icons/reboot.png" class="icon_right"/>Reboot (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely suspend this computer?')) DevicePage.controlComputer(-3);"><img alt="" src="images/icons/suspend.png" class="icon_right"/>Suspend (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely hibernate this computer?')) DevicePage.controlComputer(-4);"><img alt="" src="images/icons/hibernate.png" class="icon_right"/>Hibernate (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely shutdown this computer?')) DevicePage.controlComputer(-5);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Shutdown (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely power off this computer?')) DevicePage.controlComputer(-6);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Power Off (Forced)</li>
</ul>
</div>
<div id="device_control_ac">
<h2><img alt="" src="images/icons/ac.png" class="icon_title"/><strong>Control This AC Node</strong></h2>
<ul>
<li>Power<div id="device_control_ac_toggle"></div></li>
</ul>
</div>

<!--
<div id="device_data">
<h2><img alt="" src="images/icons/data.png" class="icon_title"/><strong>Data Feedback</strong></h2>
<ul id="device_data_hidden">
<li onclick="DevicePage.showLiveData();"><img alt="" src="images/icons/show.png" class="icon_right"/>Show Live Data</li>
</ul>
<ul id="device_data_shown">
<li><center><canvas id="device_canvas" width="270" height="100" style="overflow: hidden; margin: 10px 0;"></canvas></center></li>
<li><strong>Current: </strong><span id="device_data_current">0.0</span></li>
<li onclick="DevicePage.hideLiveData();"><img alt="" src="images/icons/hide.png" class="icon_right"/>Hide Live Data</li>
</ul>
</div>
<div id="device_control_acnode">
<h2><img alt="" src="images/icons/ac.png" class="icon_title"/><strong>Control This AC Node</strong></h2>
<ul>
<li>Power<div id="device_control_acnode_switch"></div></li>
</ul>
</div>

<div id="device_control_computernode">
<h2><img alt="" src="images/icons/computer.png" class="icon_title"/><strong>Control This Computer</strong></h2>
<ul>
<li onclick="if(confirm('Are you sure to log off the user on this computer?')) DevicePage.controlComputerNode(1);"><img alt="" src="images/icons/logoff.png" class="icon_right"/>Log Off</li>
<li onclick="if(confirm('Are you sure to reboot this computer?')) DevicePage.controlComputerNode(2);"><img alt="" src="images/icons/reboot.png" class="icon_right"/>Reboot</li>
<li onclick="if(confirm('Are you sure to suspend this computer?')) DevicePage.controlComputerNode(3);"><img alt="" src="images/icons/suspend.png" class="icon_right"/>Suspend</li>
<li onclick="if(confirm('Are you sure to hibernate this computer?')) DevicePage.controlComputerNode(4);"><img alt="" src="images/icons/hibernate.png" class="icon_right"/>Hibernate</li>
<li onclick="if(confirm('Are you sure to shutdown this computer?')) DevicePage.controlComputerNode(5);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Shutdown</li>
<li onclick="if(confirm('Are you sure to power off this computer?')) DevicePage.controlComputerNode(6);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Power Off</li>
</ul>
<ul>
<li onclick="if(confirm('Are you sure to forcely log off the user on this computer?')) DevicePage.controlComputerNode(-1);"><img alt="" src="images/icons/logoff.png" class="icon_right"/>Log Off (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely reboot this computer?')) DevicePage.controlComputerNode(-2);"><img alt="" src="images/icons/reboot.png" class="icon_right"/>Reboot (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely suspend this computer?')) DevicePage.controlComputerNode(-3);"><img alt="" src="images/icons/suspend.png" class="icon_right"/>Suspend (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely hibernate this computer?')) DevicePage.controlComputerNode(-4);"><img alt="" src="images/icons/hibernate.png" class="icon_right"/>Hibernate (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely shutdown this computer?')) DevicePage.controlComputerNode(-5);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Shutdown (Forced)</li>
<li onclick="if(confirm('Are you sure to forcely power off this computer?')) DevicePage.controlComputerNode(-6);"><img alt="" src="images/icons/poweroff.png" class="icon_right"/>Power Off (Forced)</li>
</ul></div>-->
</div></div></body></html>