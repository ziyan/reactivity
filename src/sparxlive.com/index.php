<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Welcome to SparxLive</title>
<link rel="shortcut icon" href="/favicon.ico" />
<link rel="alternate" type="application/atom+xml" title="SparxLive Blog Updates" href="http://blog.sparxlive.com/feeds/posts/default" />
<style type="text/css">
body
{
	margin: 150px;
	padding: 0;
	font-size: 9pt;
	font-family: Arial,Helvetica,sans-serif;
	background: #FFF;
}

a { color: #666; text-decoration: none; }
a:hover, a:focus, a:active { color: #000; text-decoration: underline; }


strong
{
	font-family: "Lucida Grande","Lucida Sans Unicode",sans-serif;
	text-transform: uppercase;
	font-size: 10pt;
}

img { border:0; }

.main
{
	text-align:center;
}

.main .inner
{
	width:800px;
}

.main .inner .logo
{
	float: left;
	width: 350px;
	height: 250px;
	border-right: 1px #D0D0D0 solid;
}

.main .inner .content
{
	float:left;
	padding: 25px;
	text-align:left;
	width:360px;
}

.copyright
{
	color: #D0D0D0;
	font-size: 8pt;
	font-family: "Lucida Grande","Lucida Sans Unicode",sans-serif;
	text-transform: uppercase;
}

.copyright a
{
	color: #D0D0D0;
}
</style>
</head>

<body>
<div class="main">
<center>
<div class="inner">
<div class="logo">
<img alt="sparxlive" src="images/logo.jpg" style="width:300px; height:120px; margin-top:65px"/>
</div>
<div class="content">
<div>
<strong>Welcome</strong><br/><br/>
We are team <b>Sparx</b> from <a href="http://www.rit.edu">Rochester Institute of Technology</a>.
As <a href="http://imaginecup.com">Microsoft Imagine Cup</a> 2008 Worldwide Finalists,
we are going to represent the United States in the upcoming
World Final in Paris, France.
<br/>
<br/>
<a href="http://wiki.sparxlive.com">Visit our Wiki for more information &gt;&gt;&gt;</a>
</div>
<br/><br/>
<div style="height:90px">
<strong>Featured Project</strong><br/><br/>
<a href="http://reac.tivity.org" title="Reactivity">
<img alt="Reacitivity" src="images/reactivity_logo.jpg" style="width:60px; height:60px; float:left; margin-right:8px"/></a><a href="http://reac.tivity.org" title="Reactivity" style="color: #000"><b>Reactivity</b></a><br/>
Reactivity sensor network provides home, enterprise and city-level networking of sensor and device control systems.
</div>
<br/><br/>
<div>
<strong><a href="http://twitter.com/sparxlive/with_friends" style="color:#000">Updates</a></strong><br/>
<ul>
<?php
date_default_timezone_set('EST');
define('MAGPIE_OUTPUT_ENCODING', 'UTF-8');
define('MAGPIE_CACHE_DIR', '/home/sparxlive/sparxlive.com/cache');
define('MAGPIE_FETCH_TIME_OUT', 1);
require_once('magpierss/rss_fetch.inc');
$rss = fetch_rss('http://twitter.com/statuses/friends_timeline/14885359.rss');
$items = array_slice($rss->items, 0, 5);
foreach ($items as $item) {
echo "<li><a href=\"".$item['link']."\">".$item['title']."</a><span style=\"color: #CCC\"> - ".date("F j, G:i",strtotime($item['pubdate']))."</span></li>";
}
?>
</ul></div>
<br/>
<div>
<strong><a href="http://blog.sparxlive.com" style="color:#000">Blog</a></strong><br/>
<ul>
<?php
$rss = fetch_rss('http://blog.sparxlive.com/feeds/posts/default?alt=rss');
$items = array_slice($rss->items, 0, 5);
foreach ($items as $item) {
echo "<li><a href=\"".$item['link']."\" title=\"Posted on ".date("l, F j, Y",strtotime($item['pubdate']))."\">".$item['title']."</a></li>";
}
?>
</ul>
</div>
<br/>
<div>
<strong><a href="http://wiki.sparxlive.com" style="color:#000">Wiki Changes</a></strong><br/>
<ul>
<?php
$rss = fetch_rss('http://wiki.sparxlive.com/index.php?title=Special:Recentchanges&feed=rss');
$items = array_slice($rss->items, 0, 5);
foreach ($items as $item) {
echo "<li><a href=\"".$item['link']."\" title=\"Changed on ".date("l, F j, Y",strtotime($item['pubdate']))."\">".$item['title']."</a></li>";
}
?>
</ul>
</div>
<br/>
<div class="copyright">
&copy; 2008 <a href="http://sparxlive.com">sparxlive.com</a> • All rights reserved • <a href="mailto:contact@sparxlive.com">Contact Us</a>
</div>
</div>
</div>
</center>
</div>
<script type="text/javascript">
var gaJsHost = (("https:" == document.location.protocol) ? "https://ssl." : "http://www.");
document.write(unescape("%3Cscript src='" + gaJsHost + "google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E"));
</script>
<script type="text/javascript">
var pageTracker = _gat._getTracker("UA-363200-3");
pageTracker._initData();
pageTracker._trackPageview();
</script>
</body>
</html>
