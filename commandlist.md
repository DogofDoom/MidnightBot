######For more information and how to setup your own MidnightBot, go to: **http://github.com/Midnight-Myth/MidnightBot/**
######You can donate on paypal: `nadekodiscordbot@gmail.com` or Bitcoin `17MZz1JAqME39akMLrVT4XBPffQJ2n1EPa`

#MidnightBot List Of Commands  
Version: `MidnightBot v0.9.6027.36202`
### Help  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`-h`, `-help`, `@BotName help`, `@BotName h`, `~h`  |  Hilfe-Befehl. |  '-h !m q' or just '-h' 
`-hh`  |  Hilfe-Befehl. |  '-hh !m q' or just '-h' 
`-hgit`  |  commandlist.md Datei erstellung. **Bot Owner Only!**
`-readme`, `-guide`  |  Sendet eine readme und ein Guide verlinkt zum Channel.
`-donate`, `~donate`  |  Informationen um das Projekt zu unterstützen!
`-modules`, `.modules`  |  Listet alle Module des Bots.
`-commands`, `.commands`  |  Listet alle Befehle eines bestimmten Moduls.

### Administration  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`.grdel`  |  Aktiviert oder deaktiviert automatische Löschung von Willkommens- und Verabschiedungsnachrichten.
`.greet`  |  Aktiviert oder deaktiviert Benachrichtigungen auf dem derzeitigen Channel wenn jemand dem Server beitritt.
`.greetmsg`  |  Setzt einen neuen Gruß. Gib %user% ein, wenn du den neuen Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. |  .greetmsg Welcome to the server, %user%.
`.bye`  |  Aktiviert, oder deaktiviert Benachrichtigungen, wenn ein Benutzer den Server verlässt.
`.byemsg`  |  Setzt eine neue Verabschiedung. Gib %user% ein, wenn du den Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. |  .byemsg %user% has left the server.
`.byepm`  |  Stellt ein ob die Verabschiedung im Channel, oder per PN geschickt wird.
`.greetpm`  |  Stellt ein ob der Gruß im Channel, oder per PN geschickt wird.
`.spmom`  |  Toggles whether mentions of other offline users on your server will send a pm to them.
`.logserver`  |  Toggles logging in this channel. Logs every message sent/deleted/edited on the server. **Bot Owner Only!**
`.userpresence`  |  Starts logging to this channel when someone from the server goes online/offline/idle. **Bot Owner Only!**
`.voicepresence`  |  Toggles logging to this channel whenever someone joins or leaves a voice channel you are in right now. **Bot Owner Only!**
`.repeatinvoke`, `.repinv`  |  Zeigt die Repeat Nachricht sofort an und startet den Timer neu.
`.repeat`  |  Wiederholt eine Nachricht alle X Minuten. Falls nicht spezifiziert, Wiederholung ist deaktiviert. Benötigt 'manage messages'. | `.repeat 5 Hello there`
`.rotateplaying`, `.ropl`  |  Toggles rotation of playing status of the dynamic strings you specified earlier.
`.addplaying`, `.adpl`  |  Adds a specified string to the list of playing strings to rotate. Supported placeholders: %servers%, %users%, %playing%, %queued%, %trivia%
`.listplaying`, `.lipl`  |  Lists all playing statuses with their corresponding number.
`.removeplaying`, `.repl`, `.rmpl`  |  Removes a playing string on a given number.
`.slowmode`  |  Schaltet Slow Mode um. Wenn AN, Benutzer können nur alle 5 Sekunden eine Nachricht schicken.
`.cleanv+t`, `.cv+t`  |  Löscht alle Text-Channel die auf `-voice` enden für die keine Voicechannels gefunden werden. **Benutzung auf eigene Gefahr.**
`.voice+text`, `.v+t`  |  Erstellt einen Text-Channel für jeden Voice-Channel, welchen nur User im dazugehörigen Voice-Channel sehen können.Als Server-Owner sieht du alle Channel, zu jeder Zeit.
`.scsc`  |  Startet eine Instanz eines Cross Server Channels. Du bekommst einen Tokenden andere Benutzer benutzen müssen, um auf die selbe Instanz zu kommen.
`.jcsc`  |  Joint derzeitigen Channel einer Instanz des Cross Server Channel durch Benutzung des Tokens.
`.lcsc`  |  Verlässt Cross server Channel Instance von diesem Channel
`.asar`  |  Adds a role, or list of roles separated by whitespace(use quotations for multiword roles) to the list of self-assignable roles. |  .asar Gamer
`.rsar`  |  Removes a specified role from the list of self-assignable roles.
`.lsar`  |  Lists all self-assignable roles.
`.iam`  |  Adds a role to you that you choose. Role must be on a list of self-assignable roles. |  .iam Gamer
`.iamnot`, `.iamn`  |  Removes a role to you that you choose. Role must be on a list of self-assignable roles. |  .iamn Gamer
`.addcustreact`, `.acr`  |  Fügt eine "Custom Reaction" hinzu. **Bot Owner Only!** |  .acr "hello" I love saying hello to %user%
`.listcustreact`, `.lcr`  |  Listet alle derzeitigen "Custom Reactions" (Seitenweise mit 30 Commands je Seite). | .lcr 1
`.showcustreact`, `.scr`  |  Zeigt alle möglichen Reaktionen von einer einzigen Custom Reaction. | .scr %mention% bb
`.editcustreact`, `.ecr`  |  Bearbeitet eine Custom Reaction, Argumente sind der Custom Reaction Name, Index welcher geändert werden soll und eine (Multiwort) Nachricht.**Bot Owner Only** |  `.ecr "%mention% disguise" 2 Test 123`
`.delcustreact`, `.dcr`  |  Löscht eine "Custome Reaction" mit gegebenen Namen (und Index)
`.autoassignrole`, `.aar`  |  Fügt automatisch jedem Benutzer der dem Server joint eine Rolle zu. Gib `.aar` ein um zu deaktivieren, `.aar Rollen Name` um zu aktivieren.
`.leave`  |  Lässt  den Server verlassen. Entweder Name, oder ID benötigt. | `.leave 123123123331`
`.listincidents`, `.lin`  |  Listet alle UNGELESENEN Vorfälle und markiert sie als gelesen.
`.listallincidents`, `.lain`  |  Sendet dir eine Datei mit allen Vorfällen und markiert sie als gelesen.
`.rules`  |  Regeln
`.delmsgoncmd`  |  Ändert das automatische Löschen von erfolgreichen Befehls Aufrufen um Chat Spam zu verhindern. Server Manager Only.
`.restart`  |  Startet den Bot neu. Könnte nicht funktionieren.
`.setrole`, `.sr`  |  Setzt die Rolle für einen gegebenen Benutzer. |  .sr @User Gast
`.removerole`, `.rr`  |  Entfernt eine Rolle von einem gegebenen User. |  .rr @User Admin
`.renamerole`, `.renr`  |  Benennt eine Rolle um. Rolle die umbenannt werden soll muss muss in Liste niedriger sein als die höchste Rolle des Bots. |  `.renr "Erste Rolle" ZweiteRolle`
`.removeallroles`, `.rar`  |  Entfernt alle Rollen eines Benutzers. |  .rar @User
`.createrole`, `.cr`  |  Erstelle eine Rolle mit einem bestimmten Namen. |  `.r Awesome Role`
`.rolecolor`, `.rc`  |  Setzt die Farbe einer Rolle zur Hex, oder RGB Farb-Value die gegeben wird. |  `.color Admin 255 200 100 oderr .color Admin ffba55`
`.ban`, `.b`  |  Bannt einen erwähnten Benutzer. |  .b "@some Guy" Your behaviour is toxic.
`.softban`, `.sb`  |  Bannt und entbannt einen Benutzer per ID, oder Name mit optionaler Nachricht. |  .sb "@some Guy" Your behaviour is toxic.
`.kick`, `.k`  |  Kickt einen erwähnten User.
`.mute`  |  Mutet erwähnte Benutzer.
`.unmute`  |  Entmutet erwähnte Benutzer.
`.deafen`, `.deaf`  |  Stellt erwähnte Benutzer Taub.
`.undeafen`, `.undef`  |  Erwähnte Benutzer sind nicht mehr taub.
`.delvoichanl`, `.dvch`  |  Löscht einen Voice-Channel mit einem gegebenen Namen..
`.creatvoichanl`, `.cvch`  |  Erstellt einen neuen Voice-Channel mit einem gegebenen Namen.
`.deltxtchanl`, `.dtch`  |  Löscht einen Text-Channel mit einem gegebenen Namen.
`.creatxtchanl`, `.ctch`  |  Erstellt einen Text-Channel mit einem gegebenen Namen.
`.settopic`, `.st`  |  Setzt eine Beschreibung für den derzeitigen Channel. |  `{Prefix}st My new topic`
`.setchanlname`, `.schn`  |  Ändert den Namen des derzeitigen Channels.
`.heap`  |  Zeigt benutzten Speicher - **Bot Owner Only!**
`.getinactive`, `.gi`  |  Zeigt anzahl inaktiver Benutzer - **Bot Owner Only!**
`.prune`, `.clr`  |  `.prune` alle von MidnightBots Nachrichten, in den letzten 100 Nachrichten.`.prune X` entfernt die letzten X Nachrichten von diesem Channel (bis zu 100)`.prune @Someone` Entfernt alle Nachrichten einer Person. in den letzten 100 Nachrichten.`.prune @Someone X` Entfernt die letzen X Nachrichten einer Person in diesem Channel.\n |  `.prune` oder `.prune 5` oder `.prune @Someone` oder `.prune @Someone X`
`.die`  |  Fährt den Bot herunter und benachrichtigt Benutzer über den Neustart. **Bot Owner Only!**
`.setname`, `.newnm`  |  Gibt dem Bot einen neuen Namen. **Bot Owner Only!**
`.newavatar`, `.setavatar`  |  Setzt ein neues Profilbild für MidnightBot. **Bot Owner Only!**
`.setgame`  |  Setzt das Spiel des Bots. **Bot Owner Only!**
`.send`  |  Sendet eine Nachricht an einen Benutzer auf einem anderen Server, über den Bot. **Bot Owner Only!** |  `.send serverid|u:user_id Send this to a user!` oder `.send serverid|c:channel_id Send this to a channel!`
`.mentionrole`, `.menro`  |  Erwähnt jeden User mit einer bestimmten Rolle oder bestimmten Rollen (Getrennt mit einem ',') auf diesem Server. 'Mention everyone' Berechtigung erforderlich.
`.unstuck`  |  Löscht die Nachrichten-Liste. **Bot Owner Only!**
`.donators`  |  Liste von Leuten die dieses Projekt unterstützen.
`.donadd`  |  Fügt einen Donator zur Datenbank hinzu.
`.announce`  |  Sends a message to all servers' general channel bot is connected to.**Bot Owner Only!** |  .announce Useless spam
`.servers`  |  Zeigt alle Server an, auf denen der Bot ist.
`.leave`  |  Leaves a server with a supplied ID. |  `.leave 493243292839`
`.savechat`  |  Speichert eine Anzahl an Nachrichten in eine Textdate und sendet sie zu dir. **Bot Owner Only** |  `.chatsave 150`

### Utility  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`.remind`  |  Sendet nach einer bestimmten Zeit eine Nachricht in den Channel. Erstes Argument ist me/here/'channelname'. Zweites Argument ist die Zeit in absteigender Reihenfolge (mo>w>d>h>m) Beispiel: 1w5d3h10m. Drittes Argument ist eine (Multiwort)Nachricht.  |  `.remind me 1d5h Do something` or `.remind #general Start now!`
`.remindmsg`  |  Setzt Nachricht, wenn die Erinnerung ausgelöst wird.  Verfügbare Platzhalter sind %user% - Benutzer der den Command ausgeführt hat, %message% - Nachricht spezifiziert in Erinnerung, %target% - Ziel Channel der Erinnerung. **Bot Owner Only!**
`.serverinfo`, `.sinfo`  |  Zeigt Infos über den Server, auf dem der Bot läuft. Falls kein Server ausgewählt, derzeitiger wird ausgewählt. | .sinfo Some Server
`.channelinfo`, `.cinfo`  |  Zeigt Infos über einen Channel. Wenn kein Channel ausgewählt, derzeitiger wird angegeben. | .cinfo #some-channel
`.userinfo`, `.uinfo`  |  Zeigt eine Info über den User. Wenn kein User angegeben, User der den Befehl eingibt. | .uinfo @SomeUser
`.whoplays`  |  Zeigt eine Liste von Benutzern die ein gewähltes Spiel spielen.
`.inrole`  |  Listet alle Benutzer von einer angegebenen Rolle, oder Rollen (getrennt mit einem ',') auf diesem Server.
`.checkmyperms`  |  Kontrolliere deine Berechtigungen auf diesem Server.
`.stats`  |  Zeigt ein paar Statisitken über MidnightBot.
`.dysyd`  |  Zeigt ein paar Statisitken über MidnightBot.
`.userid`, `.uid`  |  Zeigt die ID eines Benutzers.
`.channelid`, `.cid`  |  Zeigt ID des derzeitigen Channels
`.serverid`, `.sid`  |  Zeigt ID des derzeitigen Servers.
`.roles`  |  Listet alle Rollen auf diesem Server, oder die eines Benutzers wenn spezifiziert.

### Permissions  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`;chnlfilterinv`, `;cfi`  |  Aktiviert, oder deaktiviert automatische Löschung von Einladungen in diesem Channel.Falls kein Channel gewählt, derzeitige Channel. Benutze ALL um alle derzeitig existierenden Channel zu wählen. |  ;cfi enable #general-chat
`;srvrfilterinv`, `;sfi`  |  Aktiviert, oder deaktiviert automatische Löschung von Einladungenauf diesem Server. |  ;sfi disable
`;chnlfilterwords`, `;cfw`  |  Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf diesem Channel, die gebannte Wörter beinhalten.Wenn kein Channel ausgewählt, dieser hier. Benutze ALL um auf alle derzeit existierenden Channel zu aktivieren. |  ;cfw enable #general-chat
`;addfilterword`, `;afw`  |  Fügt ein neues Wort zur Liste der gefilterten Wörter hinzu. |  ;afw poop
`;rmvfilterword`, `;rfw`  |  Entfernt ein Wort von der Liste der gefilterten Wörter. |  ;rw poop
`;lstfilterwords`, `;lfw`  |  Zeigt Liste der gefilterten Wörter. |  ;lfw
`;srvrfilterwords`, `;sfw`  |  Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf dem Server, die verbotene Wörter enthalten. |  ;sfi disable
`;permrole`, `;pr`  |  Setzt eine Rolle, welche die Berechtigungen bearbeiten kann. Ohne Angabe wird die derzeitige Rolle gezeigt. Standard 'Pony'.
`;rolepermscopy`, `;rpc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einer Rolle zu einer anderen. | `;rpc Some Role ~ Some other role`
`;chnlpermscopy`, `;cpc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Channel zu einem anderen. | `;cpc Some Channel ~ Some other channel`
`;usrpermscopy`, `;upc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Benutzer, zu einem anderen. | `;upc @SomeUser ~ @SomeOtherUser`
`;verbose`, `;v`  |  Ändert ob das blocken/entblocken eines Modules/Befehls angezeigt wird. |  ;verbose true
`;serverperms`, `;sp`  |  Zeigt gebannte Berechtigungen für diesen Server.
`;roleperms`, `;rp`  |  Zeigt gebannt Berechtigungen für eine bestimmte Rolle. Kein Argument bedeutet für alle. |  ;rp AwesomeRole
`;chnlperms`, `;cp`  |  Zeigt gebannte Berechtigungen für einen bestimmten Channel. Kein Argument für derzeitigen Channel. |  ;cp #dev
`;userperms`, `;up`  |  Zeigt gebannte Berechtigungen für einen bestimmten Benutzer. Keine Argumente für sich selber. |  ;up Kwoth
`;srvrmdl`, `;sm`  |  Setzt die Berechtigung eines Moduls auf Serverlevel. |  ;sm [module_name] enable
`;srvrcmd`, `;sc`  |  Setzt die Berechtigung eines Befehls auf Serverlevel. |  ;sc [command_name] disable
`;rolemdl`, `;rm`  |  Setzt die Berechtigung eines Moduls auf Rollenlevel. |  ;rm [module_name] enable [role_name]
`;rolecmd`, `;rc`  |  Setzt die Berechtigung eines Befehls auf Rollenlevel. |  ;rc [command_name] disable [role_name]
`;chnlmdl`, `;cm`  |  Setzt die Berechtigung eines Moduls auf Channellevel. |  ;cm [module_name] enable [channel_name]
`;chnlcmd`, `;cc`  |  Setzt die Berechtigung eines Befehls auf Channellevel. |  ;cc [command_name] enable [channel_name]
`;usrmdl`, `;um`  |  Setzt die Berechtigung eines Moduls auf Benutzerlevel. |  ;um [module_name] enable [user_name]
`;usrcmd`, `;uc`  |  Setzt die Berechtigung eines Befehls auf Benutzerlevel. |  ;uc [command_name] enable [user_name]
`;allsrvrmdls`, `;asm`  |  Setzt die Berechtigung aller Module auf Serverlevel. |  ;asm [enable/disable]
`;allsrvrcmds`, `;asc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Serverlevel. |  ;asc [module_name] [enable/disable]
`;allchnlmdls`, `;acm`  |  Setzt Berechtigungen für alle Module auf Channellevel. |  ;acm [enable/disable] [channel_name]
`;allchnlcmds`, `;acc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Channellevel. |  ;acc [module_name] [enable/disable] [channel_name]
`;allrolemdls`, `;arm`  |  Setzt Berechtigung von allen Modulen auf Rollenlevel. |  ;arm [enable/disable] [role_name]
`;allrolecmds`, `;arc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Rollenlevel. |  ;arc [module_name] [enable/disable] [role_name]
`;ubl`  |  Blacklists einen Benutzer. |  ;ubl [user_mention]
`;uubl`  |  Unblacklisted einen erwähnten Benutzer. |  ;uubl [user_mention]
`;cbl`  |  Blacklists einen erwähnten Channel (#general zum Beispiel). |  ;ubl [channel_mention]
`;cubl`  |  Unblacklists einen erwähnten Channel (#general zum Beispiel). |  ;cubl [channel_mention]
`;sbl`  |  Blacklists einen Server per Name, oder ID (#general zum Beispiel). |  ;usl [servername/serverid]
`;subl`  |  Unblacklists einen erwähnten Server (#general zum Beispiel). |  ;cubl [channel_mention]

### Conversations  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`..`  |  Fügt ein neues Zitat mit Keyword (einzelnes Wort) und Nachricht (kein Limit). |  .. abc My message
`...`  |  Zeigt ein zufälliges Zitat eines Benutzers. |  .. abc
`..qdel`, `..quotedelete`  |  Löscht alle Zitate mit angegebenen Keyword. Du musst entweder der Bot-Besitzer oder der Ersteller des Quotes sein um es zu löschen. |  `..qdel abc`
`..qdelothers`, `..quotedeleteothers`  |  Löscht alle Zitate mit eigenem Namen als Keyword, welche von anderen geaddet wurden.  |  `..qdelothers`
`..qshow`  |  Zeigt alle Zitate mit angegebenen Keyword. |  `..qshow abc`
`@BotName rip`  |  Zeigt ein Grab von jemanden mit einem Startjahr |  @ rip @Someone 2000
`@BotName uptime`  |  Zeigt wie lange  schon läuft.
`@BotName die`  |  Funktioniert nur für den Owner. Fährt den Bot herunter.
`@BotName do you love me`, `@BotName do you love me?`  |  Antwortet nur dem Owner positiv.
`@BotName how are you`, `@BotName how are you?`  |  Antwortet nur positiv, wenn der Owner online ist.
`@BotName insult`  |  Beleidigt @X Person. |  @ insult @X.
`@BotName praise`  |  Lobt @X Person. |  @ praise @X.
`@BotName fire`  |  Zeigt eine unicode Feuer Nachricht. Optionaler Parameter [x] sagt ihm wie oft er das Feuer wiederholen soll. |  @ fire [x]
`@BotName slm`  |  Zeigt die Nachricht in der du in diesem Channel zuletzt erwähnt wurdest (checked die letzten 10k Nachrichten)
`@BotName dump`  |  Dumped alle Einladungen die er findet in dump.txt.** Owner Only.**
`@BotName ab`  |  Versuche 'abalabahaha' zu bekommen

### Gambling  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`$draw`  |  Zieht eine Karte vom Stapel.Wenn du eine Nummer angibst [x], werden bis zu 5 Karten vom Stapel gezogen. |  $draw [x]
`$shuffle`, `$sh`  |  Mischt alle Karten zurück in den Stapel.
`$flip`  |  Wirft eine/mehrere Münze(n) - Kopf oder Zahl, und zeigt ein Bild. |  `$flip` or `$flip 3`
`$roll`  |  Würfelt von 0-100. Wenn du eine Zahl [x] angibst werden bis zu 30 normale Würfel geworfen. Wenn du 2 Zahlen mit einem d trennst (xdy) werden x Würfel von 0 bis y geworfen. |  $roll oder $roll 7 oder $roll 3d5
`$nroll`  |  Würfelt in einer gegebenen Zahlenreichweite. |  `$nroll 5` (rolls 0-5) or `$nroll 5-15`
`$raffle`  |  Schreibt den Namen und die ID eines zufälligen Benutzers aus der Online Liste einer (optionalen) Rolle.
`$$$`  |  Überprüft, wieviele Euro du hast.
`$award`  |  Gibt jemanden eine bestimmte Anzahl an Euro. **Bot Owner Only!** |  $award 5 @Benutzer
`$take`  |  Entfernt eine bestimmte Anzahl an Euro von jemanden. **Bot Owner Only!** |  $take 5 @Benutzer
`$give`  |  Gibt jemanden eine Anzahl Euro. |  $give 5 @Benutzer
`$leaderboard`, `$lb`  |  

### Games  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`>t`  |  Startet ein Quiz. Du kannst nohint hinzufügen um Tipps zu verhindern.Erster Spieler mit 10 Punkten gewinnt. 30 Sekunden je Frage. | `>t nohint` oder `>t 5 nohint`
`>tl`  |  Zeigt eine Rangliste des derzeitigen Quiz.
`>tq`  |  Beendet Quiz nach der derzeitgen Frage.
`>typestart`  |  Startet einen Tipp-Wettbewerb.
`>typestop`  |  Stoppt einen Tipp-Wettbewerb auf dem derzeitigen Channel.
`>typeadd`  |  Fügt einen neuen Text hinzu. Owner only.
`>poll`  |  Startet eine Umfrage, Nur Personen mit 'Manage Server' Berechtigungen können dies tun. |  >poll Question?;Answer1;Answ 2;A_3
`>pollend`  |  Stoppt derzeitige Umfrage und gibt das Ergebnis aus.
`>pick`  |  Nimmt einen in diesem Channel hinterlegten Euro.
`>plant`  |  Gib einen Euro aus, um in in diesen Channel zu legen. (Wenn der Bot neustartet, oder crashed, ist der Euro verloren)
`>leet`  |  Konvertiert einen Text zu Leetspeak mit 6 (1-6) Stärke-Graden. |  >leet 3 Hallo
`>choose`  |  Sucht eine Sache aus einer Liste von Sachen aus. |  >choose Get up;Sleep;Sleep more
`>helix`  |  Stell dem allmächtigen Helix Fossil eine Ja/Nein Frage.
`>rps`  |  Spiel eine Runde Stein, Schere, Papier mit . |  >rps scissors
`>linux`  |  Prints a customizable Linux interjection. |  `{Prefix}linux Spyware Windows`

### Music  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`!next`, `!n`, `!skip`  |  Geht zum nächsten Song in der Liste. Du musst im gleichen Voice-Channel wie der Bot sein. |  `!n`
`!stop`, `!s`  |  Stoppt die Musik komplett. Bleibt im Channel.Du musst im gleichen Voice-Channel wie der Bot sein. |  `!s`
`!destroy`, `!d`  |  Stoppt die Musik komplett.
`!pause`, `!p`  |  Pausiert, oder unpausiert ein Lied.
`!queue`, `!q`, `!yq`, `!songrequest`  |  Listet einen Song mit Keyword oder Link. Bot joint dem eigenen Voice-Channel. **Du musst in einem Voice-Channel sein!**. |  `!q Dream Of Venice`
`!listqueue`, `!lq`  |  Zeigt bis zu 15 Songs per Seite. Standard Seite ist 1. |  `!lq` or `!lq 2`
`!nowplaying`, `!np`  |  Zeigt den derzeit spielenden Song.
`!volume`, `!vol`  |  Setzt die Lautstärke auf 0-100%
`!defvol`, `!dv`  |  Setzt die Standardlautstärke, wenn Musik startet. (0-100). Muss nach neustart neu eingestellt werden. |  !dv 80
`!mute`, `!min`  |  Setzt die Lautstärke auf 0%
`!max`  |  Setzt de Lautstärke auf 100% (Echte Höchstgrenze ist bei 150%).
`!half`  |  Setzt die Lautstärke auf 50%.
`!shuffle`, `!sh`  |  Mischt die derzeitige Abspielliste.
`!setgame`  |  Setzt das Spiel auf die Nummer der Lieder die gespielt werden. **Bot Owner Only!**
`!playlist`, `!pl`, `!playlistrequest`  |  Listet bis zu 50 Lieder aus einer Youtubeplaylist, oder aus einem Suchbegriff.
`soundcloudpl`, `scpl`  |  Listet eine SounCloud Playlist per Link. |  `!scpl https://soundcloud.com/saratology/sets/symphony`
`!localplaylst`, `!lopl`  |  Listet alle Lieder von einem Verzeichnis. **Bot Owner Only!**
`!radio`, `!ra`  |  Listet einen direkten Radio Stream von einem Link.
`!local`, `!lo`  |  Listet einen lokalen Song mit vollen Pfad. **Bot Owner Only!** |  `!lo C:/music/mysong.mp3`
`!move`, `!mv`  |  Verschiebt den Bot in den eigenen Voice-Channel. (Funktioniert nur, wenn schon Musik läuft)
`!remove`, `!rm`  |  Entfernt einen Song mit seiner Id, oder 'all' um die komplette Liste zu löschen.
`!movesong`, `!ms`  |  Moves a song from one position to another.
**Usage**: `! ms` 5>3
`!cleanup`  |  Bereinigt hängende Voice-Verbindung. **Bot Owner Only!**
`!reptcursong`, `!rcs`  |  Schaltet das Wiederholen des derzeitigen Liedes um.
`!rpeatplaylst`, `!rpl`  |  Schaltet das Wiederholen aller Songs in der Liste um. (Jedes beendete Lied wird an das Ende der Liste hinzugefügt).
`!save`  |  Speichert eine Playlist unter einem bestimmten Namen. Name darf nicht länger als 20 Zeichen sein und darf keine Kommas beinhalten. |  `!save classical1`
`!load`  |  Lädt eine Playlist mit bestimmten Namen. |  `!load classical1`
`!playlists`, `!pls`  |  Listet alle Playlisten. Seitenweiße. 20 je Seote. Standard-Seite ist 0. | `!pls 1`
`deleteplaylist`, `delpls`  |  Löscht eine gespeicherte Playlist. Nur wenn du sie erstellt hast, oder wenn du der Bot-Owner bist. |  `!m delpls animu-5`
`!goto`  |  Skipped zu einer bestimmten Zeit in Sekunden im aktuellen Lied.
`!getlink`, `!gl`  |  Zeigt einen Link zum derzeitigen Lied.
`!autoplay`, `!ap`  |  Toggles Autoplay - Wenn das Lied vorbei ist, listet automatisch einen verwandten YouTube-Song. (Funktioniert nur für YouTubes Songs und wenn Songliste leer ist)

### Searches  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`~lolchamp`  |  Überprüft die Statistiken eines LOL Champions. Bei Leerzeichen zusammenschreiben. Optionale Rolle. | ~lolchamp Riven or ~lolchamp Annie sup
`~lolban`  |  Zeigt die Top 12 gebannten LOL Champs. Banne diese 5 Champions und du wirst in kürzester Zeit Plat5 sein.
`~hitbox`, `~hb`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  ~hitbox SomeStreamer
`~twitch`, `~tw`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  ~twitch SomeStreamer
`~beam`, `~bm`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  ~beam SomeStreamer
`~checkhitbox`, `~chhb`  |  Checkt ob ein bestimmter User auf gerade auf Hitbox streamt. |  ~chhb SomeStreamer
`~checktwitch`, `~chtw`  |  Checkt ob ein bestimmter User auf gerade auf Twitch streamt. |  ~chtw SomeStreamer
`~checkbeam`, `~chbm`  |  Checkt ob ein bestimmter User auf gerade auf Beam streamt. |  ~chbm SomeStreamer
`~removestream`, `~rms`  |  Entfernt Benachrichtigung eines bestimmten Streamers auf diesem Channel. |  ~rms SomeGuy
`~liststreams`, `~ls`  |  Listet alle Streams die du auf diesem Server folgst. |  ~ls
`~convert`  |  Konvertiert Einheiten von>zu. |  `~convert m>km 1000`
`~convertlist`  |  Liste der kovertierbaren Dimensionen und Währungen.
`~wowjoke`  |  Get one of Kwoth's penultimate WoW jokes.
`~calculate`, `~calc`  |  Berechnet eine mathematische Angabe
`~calclist`  |  List operations of parser
`~osu`  |  Zeigt Osu-Stats für einen Spieler.
**Benutzer**:~osu Name
`~osu b`  |  Zeigt Informationen über eine Beatmap. | ~osu b https://osu.ppy.sh/s/127712
`~osu top5`  |  Zeigt die Top 5 Spiele eines Benutzers.  | ~osu top5 Name
`~pokemon`, `~poke`  |  Sucht nach einem Pokemon.
`~pokemonability`, `~pokab`  |  Sucht nach einer Pokemon Fähigkeit.
`~randomcat`, `~meow`  |  Queries http://www.random.cat/meow.
`~i`, `~image`  |  Queries .
`~memelist`  |  Zeigt eine Liste von Memes, die du mit `~memegen` benutzen kannst, von http://memegen.link/templates/
`~memegen`  |  Erstellt ein Meme von Memelist mit Top und Bottom Text. |  `~memegen biw "gets iced coffee" "in the winter"`
`~we`  |  Zeigt Wetter-Daten für eine genannte Stadt und ein Land. BEIDES IST BENÖTIGT. Wetter Api ist sehr zufällig, wenn du einen Fehler machst. |  ~we Moskau RF
`~yt`  |  Durchsucht Youtube und zeigt das erste Ergebnis.
`~ani`, `~anime`, `~aq`  |  Durchsucht anilist nach einem Anime und zeigt das erste Ergebnis.
`~imdb`  |  Durchsucht IMDB nach Filmen oder Serien und zeigt erstes Ergebnis.
`~mang`, `~manga`, `~mq`  |  Durchsucht anilist nach einem Manga und zeigt das erste Ergebnis.
`~randomcat`, `~meow`  |  Zeigt ein zufälliges Katzenbild.
`~i`  |  Zeigt das erste Ergebnis für eine Suche. Benutze ~ir für unterschiedliche Ergebnisse. |  ~i cute kitten
`~ir`  |  Zeigt ein zufälliges Bild bei einem angegeben Suchwort. |  ~ir cute kitten
`~lmgtfy`  |  Google etwas für einen Idioten.
`~hs`  |  Sucht eine Heartstone-Karte und zeigt ihr Bild. Braucht eine Weile zum beenden. | ~hs Ysera
`~ud`  |  Durchsucht das Urban Dictionary nach einem Wort. | ~ud Pineapple
`~#`  |  Durchsucht Tagdef.com nach einem Hashtag. | ~# ff
`~quote`  |  Zeigt ein zufälliges Zitat.
`~catfact`  |  Zeigt einen zufälligen Katzenfakt von <http://catfacts-api.appspot.com/api/facts>
`~yomama`, `~ym`  |  Zeigt einen zufälligen Witz von <http://api.yomomma.info/>
`~randjoke`, `~rj`  |  Zeigt einen zufälligen Witz von <http://tambal.azurewebsites.net/joke/random>
`~chucknorris`, `~cn`  |  Zeigt einen zufälligen Chuck Norris Witz von <http://tambal.azurewebsites.net/joke/random>
`~stardew`  |  Gibt einen Link zum Stardew Valley Wiki mit gegebenem Topic zurück. |  ~stardew Cow
`~magicitem`, `~mi`  |  Zeigt ein zufälliges Magic-Item von <https://1d4chan.org/wiki/List_of_/tg/%27s_magic_items>
`~revav`  |  Gibt ein Google Reverse Image Search für das Profilbild einer Person zurück.
`~revimg`  |  Gibt eine 'Google Reverse Image Search' für ein Bild von einem Link zurück.
`~safebooru`  |  Zeigt ein zufälliges Hentai Bild von safebooru  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  ~safebooru yuri +kissing
`~pony`, `~broni`  |  Shows a random image from bronibooru with a given tag. Tag is optional but preferred. (multiple tags are appended with +) |  ~pony scootaloo
`~wiki`  |  Gibt einen Wikipedia-Link zurück.
`~clr`  |  Zeigt dir die zum Hex zugehörige Farbe. |  `~clr 00ff00`
`~videocall`  |  Erstellt einen privaten <http://www.appear.in> Video Anruf Link für dich und andere erwähnte Personen. Der Link wird allen erwähnten Personen per persönlicher Nachricht geschickt.
`~av`, `~avatar`  |  Zeigt den Avatar einer erwähnten Person.
  |  ~av @X

### Extra  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`#Schinken`  |  Schinken
`#Wambo`  |  Wambo
`AND HIS NAME IS`  |  John Cena
`.kekse`  |  Verteilt Kekse an eine bestimmte Person
`~rip`  |  RIP
`~randomschinken`, `~rs`  |  Zeigt ein zufälliges Schinkenbild.
`~randomlocation`, `~rl`  |  Zeigt eine zufällige Stadt.
`~randomimage`, `~ri`  |  Zeigt ein zufälliges Bild.

### Pokegame  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`>active`  |  Zeigt das aktive Pokemon von jemandem oder einem selbst | {Prefix}active oder {Prefix}active @Someone
`>pokehelp`, `>ph`  |  Zeigt die Basis Hilfe für Pokemon Kämpfe
`>movelist`, `>ml`  |  Zeigt eine Liste der verfügbaren Angriffe. | {Prefix}movelist, {Prefix}ml, {Prefix}ml charmander
`>switch`  |  Setzt dein aktives Pokemon per Nickname | >switch mudkip
`>allmoves`, `>am`  |  Sendet dir eine private Nachticht mit allen Attacken deiner Pokemon. | {Prefix}allmoves, {Prefix}am
`>list`  |  Gibt eine Liste deiner Pokemon (6) zurück (aktives Pokemon ist unterstrichen)
`>elite4`, `>e4`  |  Zeigt die 5 stärksten Pokemon. | >elite4
`>heal`  |  Heilt dein angegebenes Pokemon (per Nicknamen) oder das aktive Pokemon der gegebenen Person. | >heal bulbasaur, >heal @user, >heal all
`>rename`, `>rn`  |  Benennt dein aktives Pokemon um. |  >rename dickbutt, >rn Mittens
`>reset`  |  Setzt deine Pokemon zurück. KANN NICHT RÜCKGÄNGIG GEMACHT WERDEN | >reset true
`>catch`  |  Versucht das derzeitige wilde Pokemon zu fangen. Du musst das Pokemon angeben, welches du ersetzen willst. Kostet einen Euro | >catch MyMudkip
`>attack`, `>`  |  Greift gegebenes Ziel mit gegebener Attacke an. |  >attack hyperbeam @user, >attack @user flame-charge, > sunny-day @user

### Translator  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`~translate`, `~trans`  |  Übersetzt Text von>zu. Von der gegebenen Sprache in die Zielsprache. |   'Prefix'trans en>de This is some text.
`~translangs`  |  Listet die verfügbaren Sprachen zur Übersetzung.

### Memes  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`~9gag`, `~9r`  |  Gets a random 9gag post
`feelsbadman`, `FeelsBadMan`  |  FeelsBadMan
`feelsgoodman`, `FeelsGoodMan`  |  FeelsGoodMan

### NSFW  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`~hentai`  |  Zeigt ein zufälliges NSFW Hentai Bild von gelbooru und danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  ~hentai yuri+kissing
`~atfbooru`, `~atf`  |  Shows a random hentai image from atfbooru with a given tag. Tag is optional but preffered. (multiple tags are appended with +) |  ~atf yuri+kissing
`~danbooru`  |  Zeigt ein zufälliges Hentai Bild von danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  ~danbooru yuri+kissing
`~r34`  |  Zeigt ein zufälliges Hentai Bild von rule34.paheal.net mit einem gegebenen Tag. |  ~r34 bacon
`~gelbooru`  |  Zeigt ein zufälliges Hentai Bild von gelbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  ~gelbooru yuri+kissing
`~rule34`  |  Zeigt ein zufälliges Hentai Bild von rule34.xx  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  ~rule34 yuri+kissing
`~e621`  |  Zeigt ein zufälliges Hentai Bild von e621.net mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze Leerzeichen für mehrere Tags. |  ~e621 yuri+kissing
`~derpi`  |  Zeigt ein zufälliges Hentai Bild von derpiboo.ru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  ~derpi yuri+kissing
`~boobs`  |  Erwachsenen Inhalt.
`~butts`, `~ass`, `~butt`  |  Erwachsenen Inhalt.

### Sounds  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`!patrick`  |  Hier ist Patrick!
`!rage`  |  Das ist jetzt nicht euer Ernst!
`!airhorn`  |  Airhorn!
`!johncena`, `!cena`  |  JOHN CENA!
`!e`, `!ethanbradberry`, `!h3h3`  |  Ethanbradberry!
`!anotha`, `!anothaone`  |  Anothaone

### ClashOfClans  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`,createwar`, `,cw`  |  Creates a new war by specifying a size (>10 and multiple of 5) and enemy clan name. | ,cw 15 The Enemy Clan
`,startwar`, `,sw`  |  Starts a war with a given number.
`,listwar`, `,lw`  |  Shows the active war claims by a number. Shows all wars in a short way if no number is specified. |  ,lw [war_number] or ,lw
`,claim`, `,call`, `,c`  |  Claims a certain base from a certain war. You can supply a name in the third optional argument to claim in someone else's place.  |  ,call [war_number] [base_number] [optional_otheruser]
`,claimfinish`, `,cf`  |  Finish your claim if you destroyed a base. Optional second argument finishes for someone else. |  ,cf [war_number] [optional_other_name]
`,unclaim`, `,uncall`, `,uc`  |  Removes your claim from a certain war. Optional second argument denotes a person in whos place to unclaim |  ,uc [war_number] [optional_other_name]
`,endwar`, `,ew`  |  Ends the war with a given index. | ,ew [war_number]

### Customreactions  
Befehl und Alternativen | Beschreibung | Benutzung
----------------|--------------|-------
`\o\`  |  Custom Reaction. | \o\
`/o/`  |  Custom Reaction. | /o/
`moveto`  |  Custom Reaction. | moveto
`comeatmebro`  |  Custom Reaction. | comeatmebro
`@BotName pat`, `<@!186940149148418048> pat`  |  Custom Reaction. | %mention% pat
`@BotName cry`, `<@!186940149148418048> cry`  |  Custom Reaction. | %mention% cry
`@BotName are you real?`, `<@!186940149148418048> are you real?`  |  Custom Reaction. | %mention% are you real?
`@BotName are you there?`, `<@!186940149148418048> are you there?`  |  Custom Reaction. | %mention% are you there?
`@BotName draw`, `<@!186940149148418048> draw`  |  Custom Reaction. | %mention% draw
`@BotName bb`, `<@!186940149148418048> bb`  |  Custom Reaction. | %mention% bb
`@BotName call`, `<@!186940149148418048> call`  |  Custom Reaction. | %mention% call
`@BotName disguise`, `<@!186940149148418048> disguise`  |  Custom Reaction. | %mention% disguise
`Da ist die Tür`  |  Custom Reaction. | Da ist die Tür
`please stop`  |  Custom Reaction. | please stop
