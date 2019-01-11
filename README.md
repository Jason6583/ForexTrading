# ForexTrading
Basic forex trading application with live chart.

Implemented technologies:
 - WCF
 - Entity Framework
 - WPF

Basic info:
Aplication is build for x64

Server have to be started before any client can log in. 
Server has to run whole time when clients are connected.

Data are stored in Microsoft SQL server for january of 2017.

Live chart is implemented inside project as User Control. User control is implemented as DDL , it could be used for other project , but project build has to be set as x64

Live chart is simulated. Data are in 1 minute time segments. Server send every 500ms data to clients , so times jumpes in minutes every 500ms in chart.

Basic function of client:
Users have to register or log in into aplication.
Users are able to buy, sell assets.
Users are able to see their porfolios.

More information is in slovak documentation inside of project.
