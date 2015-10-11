CREATE DATABASE ChatInfo
--DROP DATABASE ChatInfo
USE ChatInfo
--USE northwind

CREATE TABLE Users
(
	UserID int IDENTITY(1,1) PRIMARY KEY,
	NickName varchar(255),
	Pass varchar(255),
	Ban date	
)

CREATE TABLE Groups
(
	GroupID int IDENTITY(1,1) PRIMARY KEY,
	GroupName varchar(255)
)

CREATE TABLE MemeberOfGroups
(
	GroupID int,
	UserID int
)


CREATE TABLE Messagese
(
	MessageID int IDENTITY(1,1) PRIMARY KEY,
	Texts varchar(255),
	GroupID int,
	UserID int,
	TimeMes datetime
)

CREATE TABLE BanList
(
	UserID1 int,
	UserID2 int
)

--CREATE TABLE IndividualMessages
--(
--	UserID1 int,
--	UserID2 int,
--	Texts varchar(255)
--)

ALTER TABLE MemeberOfGroups ADD 
CONSTRAINT FK_MemeberOfGroups_Users FOREIGN KEY(UserID)
	REFERENCES Users(UserID);

ALTER TABLE MemeberOfGroups ADD 
CONSTRAINT FK_MemeberOfGroups_Groups FOREIGN KEY(GroupID)
	REFERENCES Groups(GroupID);

ALTER TABLE Messagese ADD 
CONSTRAINT FK_Messagese_Users FOREIGN KEY(UserID)
	REFERENCES Users(UserID);

ALTER TABLE Messagese ADD 
CONSTRAINT FK_Messagese_Groups FOREIGN KEY(GroupID)
	REFERENCES Groups(GroupID);

ALTER TABLE BanList ADD 
CONSTRAINT FK_BanList_Users FOREIGN KEY(UserID1)
	REFERENCES Users(UserID);

ALTER TABLE BanList ADD 
CONSTRAINT FK2_BanList_Users FOREIGN KEY(UserID2)
	REFERENCES Users(UserID);

--ALTER TABLE IndividualMessages ADD 
--CONSTRAINT FK_IndividualMessages_Users FOREIGN KEY(UserID1)
--	REFERENCES Users(UserID);

--ALTER TABLE IndividualMessages ADD 
--CONSTRAINT FK2_IndividualMessages_Users FOREIGN KEY(UserID2)
--	REFERENCES Users(UserID);

--INSERT INTO BanList (UserID1, UserID2) 
--VALUES (1, 2)

INSERT INTO Users (NickName, Pass, Ban) 
VALUES ('777', 777, '1900-01-01')

INSERT INTO Users (NickName, Pass, Ban) 
VALUES ('888', 888, '1900-01-01')

INSERT INTO Users (NickName, Pass, Ban) 
VALUES ('999', 999, '1900-01-01')

INSERT INTO Users (NickName, Pass, Ban) 
VALUES ('666', 666, '1900-01-01')

INSERT INTO Users (NickName, Pass, Ban) 
VALUES ('555', 555, '1900-01-01')

INSERT INTO Groups (GroupName) 
VALUES ('sport')

INSERT INTO Groups (GroupName) 
VALUES ('movie')

INSERT INTO Groups (GroupName) 
VALUES ('music')

INSERT INTO Groups (GroupName) 
VALUES ('education')

INSERT INTO Groups (GroupName) 
VALUES ('science')

INSERT INTO Groups (GroupName) 
VALUES ('GENERAL')

INSERT INTO MemeberOfGroups(UserID, GroupID) 
VALUES (1, 6)
INSERT INTO MemeberOfGroups(UserID, GroupID) 
VALUES (2, 6)
INSERT INTO MemeberOfGroups(UserID, GroupID) 
VALUES (3, 6)
INSERT INTO MemeberOfGroups(UserID, GroupID) 
VALUES (4, 6)
INSERT INTO MemeberOfGroups(UserID, GroupID) 
VALUES (5, 6)

--insert into Messagese (GroupID, Texts, UserID, TimeMes)
--values (6, 'sdc', 1, '03.01.2015 20:40:59')

select COUNT(*) from Messagese

select * from Users;
select * from Messagese;
select * from Groups;
select * from MemeberOfGroups;
select * from BanList;
--select * from IndividualMessages;

--delete from Users
--delete from Messagese
--delete from Groups
--delete from MemeberOfGroups
--delete from BanList
--delete from IndividualMessages




--SELECT SCOPE_IDENTITY() from Users;
--select IDENT_CURRENT('Users') as ID_cur

--select * from Users where NickName=888;

--delete from Users where UserID=8 or UserID=1016;
--delete from MemeberOfGroups where UserID=1016 or UserID=1016;

--delete from BanList where UserID1=2 and UserID2=1;

--update Users set Ban = '1900-01-01';

--INSERT INTO MemeberOfGroups(UserID, GroupID) 
--VALUES (1, 6)
--INSERT INTO MemeberOfGroups(UserID, GroupID) 
--VALUES (2, 6)