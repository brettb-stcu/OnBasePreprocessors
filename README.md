# OnBaseReports
A collection of project used to write OnBase reports directly to OnBase from various sources.
## CSTickingReport
Connects to OnBase to retrieve a copy of the latest ATM: Reports > <i>ATM81000 - EFT Transaction Ticking Report</i> for <b>Card Services</b>. The 
Ticking Report is a text report produced by CO-OP on a daily basis listing transactions that could not be processed due to the
state of eithe system (CO-OP or PHX). If either system is in maintenance mode transactions fail and must be manually processed
by Card Services. CSTickingReport formats the CO-OP text report as a MS Excel Spreadsheet and then writes that spreadsheet back to 
OnBase in the <i>ATM81000 - EFT TRansaction Ticking Report - Spreadsheet</i>.
## HEApplicationsExtract
Connects to OnBase to retrieve all Home Equity Applications returned by the <i>HE Application Search</i> Custom Query within the
date parameters specified in App.Config numberOfDaysExtracted key. Those applications are Unity Forms which are stored as XML 
objects within the database and therefore need to be serialized for <b>Lending Administration</b> to do any aggregate reporting
on the data. HEApplicationsExtract formats the data as an MS Excel Spreadsheet and then writes that spreadsheet back to <i>Home Equity
Loan Applications > Home Equity Application Data Extract</i>.
