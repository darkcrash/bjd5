#!/usr/local/bin/perl

print "Content-type: text/html\n\n";

foreach $key (keys %ENV) {
	print "<b>$key</b> �F $ENV{$key}<br>\n";
}

exit;