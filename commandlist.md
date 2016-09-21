######For more information and how to setup your own MidnightBot, go to: **http://github.com/Midnight-Myth/MidnightBot/**
######You can donate on paypal: `nadekodiscordbot@gmail.com` or Bitcoin `17MZz1JAqME39akMLrVT4XBPffQJ2n1EPa`

#MidnightBot List Of Commands  
Version: `MidnightBot v0.9.6108.26944`
### Help  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`-h`, `-help`, `@BotName help`, `@BotName h`, `~h`  |  Hilfe-Befehl. |  `-h !m q` oder einfach `-h`
`-hgit`  |  commandlist.md Datei erstellung. **Bot Owner Only!** |  `-hgit`
`-readme`, `-guide`  |  Sendet eine readme und ein Guide verlinkt zum Channel. |  `-readme` or `-guide`
`-donate`, `~donate`  |  Informationen um das Projekt zu unterstützen! |  `-donate` or `-donate`
`-modules`, `.modules`  |  Listet alle Module des Bots. |  `-modules` or `.modules`
`-commands`, `.commands`  |  Listet alle Befehle eines bestimmten Moduls. |  `-commands` or `.commands`

### Administration  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`.grdel`  |  Aktiviert oder deaktiviert automatische Löschung von Willkommens- und Verabschiedungsnachrichten. |  `.grdel`
`.greet`  |  Aktiviert oder deaktiviert Benachrichtigungen auf dem derzeitigen Channel wenn jemand dem Server beitritt. |  `.greet`
`.greetmsg`  |  Setzt einen neuen Gruß. Gib %user% ein, wenn du den neuen Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. |  `.greetmsg Welcome to the server, %user%.´
`.bye`  |  Aktiviert, oder deaktiviert Benachrichtigungen, wenn ein Benutzer den Server verlässt. |  `.bye`
`.byemsg`  |  Setzt eine neue Verabschiedung. Gib %user% ein, wenn du den Benutzer erwähnen möchtest. Ohne nachfolgende Nachricht, zeigt es die derzeitige Nachricht. |  `.byemsg %user% has left the server.´
`.byepm`  |  Stellt ein ob die Verabschiedung im Channel, oder per PN geschickt wird. |  `.byepm`
`.greetpm`  |  Stellt ein ob der Gruß im Channel, oder per PN geschickt wird. |  `.greetpm`
`.spmom`  |  Toggles whether mentions of other offline users on your server will send a pm to them. |  `.spmom`
`.logserver`  |  Toggles logging in this channel. Logs every message sent/deleted/edited on the server. **Bot Owner Only!** |  `.logserver`
`.logignore`  |  Toggles whether the .logserver command ignores this channel. Useful if you have hidden admin channel and public log channel. |  `.logignore`
`.userpresence`  |  Starts logging to this channel when someone from the server goes online/offline/idle. **Bot Owner Only!** |  `.userpresence`
`.voicepresence`  |  Toggles logging to this channel whenever someone joins or leaves a voice channel you are in right now. **Bot Owner Only!** |  `.voicerpresence`
`.repeatinvoke`, `.repinv`  |  Zeigt die Repeat Nachricht sofort an und startet den Timer neu. |  `.repinv`
`.repeat`  |  Wiederholt eine Nachricht alle X Minuten. Falls nicht spezifiziert, Wiederholung ist deaktiviert. Benötigt 'manage messages'. |  `.repeat 5 Hello there`
`.rotateplaying`, `.ropl`  |  Toggles rotation of playing status of the dynamic strings you specified earlier. |  `.ropl`
`.addplaying`, `.adpl`  |  Adds a specified string to the list of playing strings to rotate. Supported placeholders: %servers%, %users%, %playing%, %queued%, %trivia% |  `.adpl`
`.listplaying`, `.lipl`  |  Lists all playing statuses with their corresponding number. |  `.lipl`
`.removeplaying`, `.repl`, `.rmpl`  |  Removes a playing string on a given number. |  `.rmpl`
`.slowmode`  |  Schaltet Slow Mode um. Wenn AN, Benutzer können nur alle 5 Sekunden eine Nachricht schicken. |  `.slowmode`
`.cleanv+t`, `.cv+t`  |  Löscht alle Text-Channel die auf `-voice` enden für die keine Voicechannels gefunden werden. **Benutzung auf eigene Gefahr.** |  `.cleanv+t`
`.voice+text`, `.v+t`  |  Erstellt einen Text-Channel für jeden Voice-Channel, welchen nur User im dazugehörigen Voice-Channel sehen können.Als Server-Owner sieht du alle Channel, zu jeder Zeit. |  `.voice+text`
`.scsc`  |  Startet eine Instanz eines Cross Server Channels. Du bekommst einen Token den andere Benutzer benutzen müssen, um auf die selbe Instanz zu kommen.. |  `.scsc`
`.jcsc`  |  Joint derzeitigen Channel einer Instanz des Cross Server Channel durch Benutzung des Tokens. |  `.jcsc`
`.lcsc`  |  Verlässt Cross server Channel Instance von diesem Channel. |  `.lcsc`
`.asar`  |  Adds a role, or list of roles separated by whitespace(use quotations for multiword roles) to the list of self-assignable roles. |  ´.asar Gamer´
`.rsar`  |  Removes a specified role from the list of self-assignable roles. |  `.rsar`
`.lsar`  |  $Lists all self-assignable roles. |  `{Prefix}lsar`
`.togglexclsar`, `.tesar`  |  Ändert ob die Self-Assigned Roles exklusiv, oder nicht exklusiv sind. |  `.tesar`
`.iam`  |  Adds a role to you that you choose. Role must be on a list of self-assignable roles. |  ´.iam Gamer´
`.iamnot`, `.iamn`  |  Removes a role to you that you choose. Role must be on a list of self-assignable roles. |  ´.iamn Gamer´
`.addcustreact`, `.acr`  |  Fügt eine "Custom Reaction" hinzu. **Bot Owner Only!** |  `.acr "hello" I love saying hello to %user%`
`.listcustreact`, `.lcr`  |  Listet Custom Reactions auf(Seitenweise mit 30 Befehlen per Seite). Benutze 'all' anstatt einer Seitenzahl um alle Custom Reactions per Privater Nachricht zu erhalten. |  `.lcr 1`
`.showcustreact`, `.scr`  |  Zeigt alle möglichen Reaktionen von einer einzigen Custom Reaction. |  `.scr %mention% bb`
`.editcustreact`, `.ecr`  |  Bearbeitet eine Custom Reaction, Argumente sind der Custom Reaction Name, Index welcher geändert werden soll und eine (Multiwort) Nachricht.**Bot Owner Only** |  `.ecr "%mention% disguise" 2 Test 123`
`.delcustreact`, `.dcr`  |  Löscht eine "Custome Reaction" mit gegebenen Namen (und Index). |  `.dcr index`
`.autoassignrole`, `.aar`  |  Fügt automatisch jedem Benutzer der dem Server joint eine Rolle zu. Gib `.aar` ein um zu deaktivieren, `.aar Rollen Name` um zu aktivieren.
`.leave`  |  Lässt  den Server verlassen. Entweder Name, oder ID benötigt. |  `.leave 123123123331`
`.listincidents`, `.lin`  |  Listet alle UNGELESENEN Vorfälle und markiert sie als gelesen. |  `.lin`
`.listallincidents`, `.lain`  |  Sendet dir eine Datei mit allen Vorfällen und markiert sie als gelesen. |  `.lain`
`.rules`  |  Regeln
`.delmsgoncmd`  |  Ändert das automatische Löschen von erfolgreichen Befehls Aufrufen um Chat Spam zu verhindern. Server Manager Only. |  `.delmsgoncmd`
`.restart`  |  Startet den Bot neu. Könnte nicht funktionieren. |  `.restart`
`.setrole`, `.sr`  |  Setzt die Rolle für einen gegebenen Benutzer. |  `.sr @User Gast`
`.removerole`, `.rr`  |  Entfernt eine Rolle von einem gegebenen User. |  `.rr @User Admin`
`.renamerole`, `.renr`  |  Benennt eine Rolle um. Rolle die umbenannt werden soll muss muss in Liste niedriger sein als die höchste Rolle des Bots. |  `.renr "Erste Rolle" ZweiteRolle`
`.removeallroles`, `.rar`  |  Entfernt alle Rollen eines Benutzers. |  `.rar @User`
`.createrole`, `.cr`  |  Erstelle eine Rolle mit einem bestimmten Namen. |  `.r Awesome Role`
`.rolecolor`, `.rc`  |  Setzt die Farbe einer Rolle zur Hex, oder RGB Farb-Value die gegeben wird. |  `.color Admin 255 200 100 oder .color Admin ffba55`
`.ban`, `.b`  |  Bannt einen erwähnten Benutzer. |  `.b "@some Guy" Your behaviour is toxic.`
`.softban`, `.sb`  |  Bannt und entbannt einen Benutzer per ID, oder Name mit optionaler Nachricht. |  `.sb "@some Guy" Your behaviour is toxic.`
`.kick`, `.k`  |  Kickt einen erwähnten User. |  `.k "@some Guy" Your behaviour is toxic.`
`.mute`  |  Mutet erwähnte Benutzer. |  `.mute "@Someguy"` oder `.mute "@Someguy" "@Someguy"`
`.unmute`  |  Entmutet erwähnte Benutzer. |  `.unmute "@Someguy"` oder `.unmute "@Someguy" "@Someguy"`
`.deafen`, `.deaf`  |  Stellt erwähnte Benutzer Taub. |  `.deaf "@Someguy"` oder `.deaf "@Someguy" "@Someguy"`
`.undeafen`, `.undef`  |  Erwähnte Benutzer sind nicht mehr taub. |  `.undef "@Someguy"` oder `.undef "@Someguy" "@Someguy"`
`.delvoichanl`, `.dvch`  |  Löscht einen Voice-Channel mit einem gegebenen Namen. |  `.dvch VoiceChannelName`
`.creatvoichanl`, `.cvch`  |  Erstellt einen neuen Voice-Channel mit einem gegebenen Namen. |  `.cvch VoiceChannelName`
`.deltxtchanl`, `.dtch`  |  Löscht einen Text-Channel mit einem gegebenen Namen. |  `.dtch TextChannelName`
`.creatxtchanl`, `.ctch`  |  Erstellt einen Text-Channel mit einem gegebenen Namen. |  `.ctch TextChannelName`
`.settopic`, `.st`  |  Setzt eine Beschreibung für den derzeitigen Channel. |  `.st My new topic`
`.setchanlname`, `.schn`  |  Ändert den Namen des derzeitigen Channels. |  `.schn NewName`
`.heap`  |  Zeigt benutzten Speicher - **Bot Owner Only!** |  `.heap`
`.getinactive`, `.gi`  |  Zeigt anzahl inaktiver Benutzer - **Bot Owner Only!**
`.prune`, `.clr`  |  `.prune` entfernt alle von MidnightBots Nachrichten, in den letzten 100 Nachrichten.`.prune X` entfernt die letzten X Nachrichten von diesem Channel (bis zu 100)`.prune @Someone` Entfernt alle Nachrichten einer Person. in den letzten 100 Nachrichten.`.prune @Someone X` Entfernt die letzen X Nachrichten einer Person in diesem Channel.| `.prune` oder `.prune 5` oder `.prune @Someone` oder `.prune @Someone X`
`.die`  |  Fährt den Bot herunter und benachrichtigt Benutzer über den Neustart. **Bot Owner Only!** |  `.die`
`.setname`, `.newnm`  |  Gibt dem Bot einen neuen Namen. **Bot Owner Only!** |  `.newnm BotName`
`.newavatar`, `.setavatar`  |  Setzt ein neues Profilbild für MidnightBot. **Bot Owner Only!** |  `.setavatar https://i.ytimg.com/vi/WDudkR1eTMM/maxresdefault.jpg`
`.setgame`  |  Setzt das Spiel des Bots. **Bot Owner Only!** |  `.setgame Playing with Midnight`
`.send`  |  Sendet eine Nachricht an einen Benutzer auf einem anderen Server, über den Bot. **Bot Owner Only!** |  `.send serverid|u:user_id Send this to a user!` oder `.send serverid|c:channel_id Send this to a channel!`
`.mentionrole`, `.menro`  |  Erwähnt jeden User mit einer bestimmten Rolle oder bestimmten Rollen (Getrennt mit einem ',') auf diesem Server. 'Mention everyone' Berechtigung erforderlich. |  `.menro RoleName`
`.unstuck`  |  Löscht die Nachrichten-Liste. **Bot Owner Only!** |  `.unstuck`
`.donators`  |  Liste von Leuten die dieses Projekt unterstützen.
`.donadd`  |  Fügt einen Donator zur Datenbank hinzu. |  `.donadd Donate Amount`
`.sendmsg`  |  Sendet eine Private Nachricht an einen User vom Bot aus.**Bot Owner Only** |  `.sendmsg @Username Nachricht`
`.announce`  |  Sends a message to all servers' general channel bot is connected to.**Bot Owner Only!** |  `.announce Useless spam`
`.servers`  |  Zeigt alle Server an, auf denen der Bot ist.
`.savechat`  |  Speichert eine Anzahl an Nachrichten in eine Textdate und sendet sie zu dir. **Bot Owner Only** |  `.savechat 150`

### Utility  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`.remind`  |  Sendet nach einer bestimmten Zeit eine Nachricht in den Channel. Erstes Argument ist me/here/'channelname'. Zweites Argument ist die Zeit in absteigender Reihenfolge (mo>w>d>h>m) Beispiel: 1w5d3h10m. Drittes Argument ist eine (Multiwort)Nachricht.  |  `.remind me 1d5h Do something` oder `.remind #general Start now!`
`.remindmsg`  |  Setzt Nachricht, wenn die Erinnerung ausgelöst wird.  Verfügbare Platzhalter sind %user% - Benutzer der den Command ausgeführt hat, %message% - Nachricht spezifiziert in Erinnerung, %target% - Ziel Channel der Erinnerung. **Bot Owner Only!** |  `.remindmsg do something else`
`.serverinfo`, `.sinfo`  |  Zeigt Infos über den Server, auf dem der Bot läuft. Falls kein Server ausgewählt, derzeitiger wird ausgewählt. |  `.sinfo Some Server`
`.channelinfo`, `.cinfo`  |  Zeigt Infos über einen Channel. Wenn kein Channel ausgewählt, derzeitiger wird angegeben. |  `.cinfo #some-channel`
`.userinfo`, `.uinfo`  |  Zeigt eine Info über den User. Wenn kein User angegeben, User der den Befehl eingibt. |  `.uinfo @SomeUser`
`.whoplays`  |  Zeigt eine Liste von Benutzern die ein gewähltes Spiel spielen. |  `.whoplays Overwatch`
`.inrole`  |  Listet alle Benutzer von einer angegebenen Rolle, oder Rollen (getrennt mit einem ',') auf diesem Server. Wenn die Liste zu lange für eine Nachricht ist, brauchst du Manage Messages Berechtigungen. |  `.inrole Role`
`.checkmyperms`  |  Kontrolliere deine Berechtigungen auf diesem Server. |  `.checkmyperms`
`.stats`  |  Zeigt ein paar Statisitken über MidnightBot. |  `.stats`
`.dysyd`  |  Zeigt ein paar Statisitken über MidnightBot. |  `.dysyd`
`.userid`, `.uid`  |  Zeigt die ID eines Benutzers. |  `.uid` oder `.uid "@SomeGuy"`
`.channelid`, `.cid`  |  Zeigt ID des derzeitigen Channels |  `.cid`
`.serverid`, `.sid`  |  Zeigt ID des derzeitigen Servers. |  `.sid`
`.roles`  |  Listet alle Rollen auf diesem Server, oder die eines Benutzers wenn spezifiziert. |  `.roles`
`.channeltopic`, `.ct`  |  Sends current channel's topic as a message. |  `.ct`

### Permissions  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`;chnlfilterinv`, `;cfi`  |  Aktiviert, oder deaktiviert automatische Löschung von Einladungen in diesem Channel.Falls kein Channel gewählt, derzeitige Channel. Benutze ALL um alle derzeitig existierenden Channel zu wählen. |  `;cfi enable #general-chat`
`;srvrfilterinv`, `;sfi`  |  Aktiviert, oder deaktiviert automatische Löschung von Einladungenauf diesem Server. |  `;sfi disable`
`;chnlfilterwords`, `;cfw`  |  Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf diesem Channel, die gebannte Wörter beinhalten.Wenn kein Channel ausgewählt, dieser hier. Benutze ALL um auf alle derzeit existierenden Channel zu aktivieren. |  `;cfw enable #general-chat`
`;addfilterword`, `;afw`  |  Fügt ein neues Wort zur Liste der gefilterten Wörter hinzu. |  `;afw poop`
`;rmvfilterword`, `;rfw`  |  Entfernt ein Wort von der Liste der gefilterten Wörter. |  `;rfw poop`
`;lstfilterwords`, `;lfw`  |  Zeigt Liste der gefilterten Wörter. |  `;lfw`
`;srvrfilterwords`, `;sfw`  |  Aktiviert, oder deaktiviert automatische Löschung von Nachrichten auf dem Server, die verbotene Wörter enthalten. |  `;sfi disable`
`;permrole`, `;pr`  |  Setzt eine Rolle, welche die Berechtigungen bearbeiten kann. Ohne Angabe wird die derzeitige Rolle gezeigt. Standard 'Pony'.
`;rolepermscopy`, `;rpc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einer Rolle zu einer anderen. |  `;rpc Some Role ~ Some other role`
`;chnlpermscopy`, `;cpc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Channel zu einem anderen. |  `;cpc Some Channel ~ Some other channel`
`;usrpermscopy`, `;upc`  |  Kopiert BOT BERECHTIGUNGEN (nicht Discord Berechtigungen) von einem Benutzer, zu einem anderen. |  `;upc @SomeUser ~ @SomeOtherUser`
`;verbose`, `;v`  |  Ändert ob das blocken/entblocken eines Modules/Befehls angezeigt wird. |  `;verbose true`
`;serverperms`, `;sp`  |  Zeigt gebannte Berechtigungen für diesen Server.
`;roleperms`, `;rp`  |  Zeigt gebannt Berechtigungen für eine bestimmte Rolle. Kein Argument bedeutet für alle. |  `;rp AwesomeRole`
`;chnlperms`, `;cp`  |  Zeigt gebannte Berechtigungen für einen bestimmten Channel. Kein Argument für derzeitigen Channel. |  `;cp #dev`
`;userperms`, `;up`  |  Zeigt gebannte Berechtigungen für einen bestimmten Benutzer. Keine Argumente für sich selber. |  `;up Kwoth`
`;srvrmdl`, `;sm`  |  Setzt die Berechtigung eines Moduls auf Serverlevel. |  `;sm "module name" enable`
`;srvrcmd`, `;sc`  |  Setzt die Berechtigung eines Befehls auf Serverlevel. |  `;sc "command name" disable`
`;rolemdl`, `;rm`  |  Setzt die Berechtigung eines Moduls auf Rollenlevel. |  `;rm "module name" enable MyRole`
`;rolecmd`, `;rc`  |  Setzt die Berechtigung eines Befehls auf Rollenlevel. |  `;rc "command name" disable MyRole`
`;chnlmdl`, `;cm`  |  Setzt die Berechtigung eines Moduls auf Channellevel. |  `;cm "module name" enable SomeChannel`
`;chnlcmd`, `;cc`  |  Setzt die Berechtigung eines Befehls auf Channellevel. |  `;cc "command name" enable SomeChannel`
`;usrmdl`, `;um`  |  Setzt die Berechtigung eines Moduls auf Benutzerlevel. |  `;um "module name" enable [user_name]`
`;usrcmd`, `;uc`  |  Setzt die Berechtigung eines Befehls auf Benutzerlevel. |  `;uc "command name" enable [user_name]`
`;allsrvrmdls`, `;asm`  |  Setzt die Berechtigung aller Module auf Serverlevel. |  `;asm [enable/disable]`
`;allsrvrcmds`, `;asc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Serverlevel. |  `;asc "module name" [enable/disable]`
`;allchnlmdls`, `;acm`  |  Setzt Berechtigungen für alle Module auf Channellevel. |  `;acm [enable/disable] SomeChannel`
`;allchnlcmds`, `;acc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Channellevel. |  `;acc "module name" [enable/disable] SomeChannel`
`;allrolemdls`, `;arm`  |  Setzt Berechtigung von allen Modulen auf Rollenlevel. |  `;arm [enable/disable] MyRole`
`;allrolecmds`, `;arc`  |  Setzt Berechtigungen für alle Befehle eines bestimmten Moduls auf Rollenlevel. |  `;arc "module name" [enable/disable] MyRole`
`;ubl`  |  Blacklists einen Benutzer. |  `;ubl [user_mention]`
`;uubl`  |  Unblacklisted einen erwähnten Benutzer. |  `;uubl [user_mention]`
`;cbl`  |  Blacklists einen erwähnten Channel (#general zum Beispiel). |  `;cbl #some_channel`
`;cubl`  |  Unblacklists einen erwähnten Channel (#general zum Beispiel). |  `;cubl #some_channel`
`;sbl`  |  Blacklists einen Server per Name, oder ID (#general zum Beispiel). |  `;sbl [servername/serverid]`
`;subl`  |  Unblacklists einen erwähnten Server (#general zum Beispiel). |  `;subl #some_channel`
`;cmdcooldown`, `;cmdcd`  |  Setzt einen Cooldown für einen Befehl per Benutzer. Setze auf 0, um den Cooldown zu entfernen. |  `;cmdcd "some cmd" 5`
`;allcmdcooldowns`, `;acmdcds`  |  Zeigt eine Liste aller Befehle und Ihrer Cooldowns.

### Conversations  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`..`  |  Fügt ein neues Zitat mit Keyword (einzelnes Wort) und Nachricht (kein Limit). |  `.. abc My message`
`...`  |  Zeigt ein zufälliges Zitat eines Benutzers. |  `... abc`
`..qdel`, `..quotedelete`  |  Löscht alle Zitate mit angegebenen Keyword. Du musst entweder der Bot-Besitzer oder der Ersteller des Quotes sein um es zu löschen. |  `..qdel abc`
`..qdelothers`, `..quotedeleteothers`  |  Löscht alle Zitate mit eigenem Namen als Keyword, welche von anderen geaddet wurden.  |  `..qdelothers`
`..qshow`  |  Zeigt alle Zitate mit angegebenen Keyword. |  `..qshow abc`
`@BotName rip`  |  Zeigt ein Grab von jemanden mit einem Startjahr |  @ rip @Someone 2000
`@BotName die`  |  Funktioniert nur für den Owner. Fährt den Bot herunter. |  `@ die`
`@BotName do you love me`, `@BotName do you love me?`  |  Antwortet nur dem Owner positiv. |  `@ do you love me`
`@BotName how are you`, `@BotName how are you?`  |  Antwortet nur positiv, wenn der Owner online ist. |  `@ do you love me`
`@BotName insult`  |  Beleidigt @X Person. |  @ insult @X.
`@BotName praise`  |  Lobt @X Person. |  @ praise @X.
`@BotName fire`  |  Zeigt eine unicode Feuer Nachricht. Optionaler Parameter [x] sagt ihm wie oft er das Feuer wiederholen soll. |  `@ fire [x]`
`@BotName dump`  |  Dumped alle Einladungen die er findet in dump.txt.** Owner Only.** |  `@ dump`
`@BotName ab`  |  Versuche 'abalabahaha' zu bekommen.| `@ ab`

### Gambling  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`$draw`  |  Zieht eine Karte vom Stapel.Wenn du eine Nummer angibst [x], werden bis zu 5 Karten vom Stapel gezogen. |  `$draw [x]´
`$shuffle`, `$sh`  |  Mischt alle Karten zurück in den Stapel. |  `$shuffle`
`$flip`  |  Wirft eine/mehrere Münze(n) - Kopf oder Zahl, und zeigt ein Bild. |  `$flip` or `$flip 3`
`$betflip`, `$bf`  |  Wette auf das Ergebnis: Kopf, oder Zahl. Beim richtigen Raten werden die gesetzten Euro verdoppelt. |  `$bf 5 kopf` or `$bf 3 z`
`$roll`  |  Rolls 0-100. If you supply a number [x] it rolls up to 30 normal dice. If you split 2 numbers with letter d (xdy) it will roll x dice from 1 to y. |  `$roll` or `$roll 7` or `$roll 3d5`
`$rolluo`  |  Rolls 0-100. If you supply a number [x] it rolls up to 30 normal dice (unordered). If you split 2 numbers with letter d (xdy) it will roll x dice from 1 to y. |  `$roll` or `$roll` 7 or `$roll 3d5`
`$nroll`  |  Rolls in a given range. |  `$nroll 5` (rolls 0-5) or `$nroll 5-15`
`$race`  |  Startet ein neues Tier-Rennen. |  `$race`
`$joinrace`, `$jr`  |  Tritt einem Rennen bei. Du kannst eine Anzahl an Euro zum Wetten setzen (Optional). Du bekommst deine Wette*(Teilnehmer-1) zurück, wenn du gewinnst. |  `$jr` oder `$jr 5`
`$raffle`  |  Schreibt den Namen und die ID eines zufälligen Benutzers aus der Online Liste einer (optionalen) Rolle. |  `$raffle` oder `$raffle RoleName`
`$$$`  |  Überprüft, wieviele Euro du hast.
`$award`  |  Gibt jemanden eine bestimmte Anzahl an Euro. **Bot Owner Only!** |  `$award 5 @Benutzer`
`$dailymoney`  |  Tägliches Geld (20 Euro, wird um 0 Uhr zurückgesetzt.) |  `$dailymoney`
`$take`  |  Entfernt eine bestimmte Anzahl an Euro von jemanden. **Bot Owner Only!** |  `$take 1 "@someguy"`
`$give`  |  Gibt jemanden eine Anzahl Euro. | `$give 1 "@SomeGuy"`
`$betroll`, `$br`  |  Wettet einen bestimmten Betrag an Euro und wirft einen Würfel. Bei über 66 Punkten: x2 Euro, über 90 Punkte: x3 und 100 x10. |  `$br 5`
`$leaderboard`, `$lb`  |  Displays bot currency leaderboard |  $lb

### Games  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`>t`  |  Startet ein Quiz. Du kannst nohint hinzufügen um Tipps zu verhindern.Erster Spieler mit 10 Punkten gewinnt. 30 Sekunden je Frage. | `>t nohint` oder `>t 5 nohint`
`>tl`  |  Zeigt eine Rangliste des derzeitigen Quiz. |  `>tl`
`>tq`  |  Beendet Quiz nach der derzeitgen Frage. |  `>tq`
`>typestart`  |  Startet einen Tipp-Wettbewerb. |  `>typestart`
`>typestop`  |  Stoppt einen Tipp-Wettbewerb auf dem derzeitigen Channel. |  `>typestop`
`>typeadd`  |  Fügt einen neuen Text hinzu. Owner only. |  `>typeadd wordswords`
`>poll`  |  Startet eine Umfrage, Nur Personen mit 'Manage Server' Berechtigungen können dies tun. |  `>poll Question?;Answer1;Answ 2;A_3`
`>pollend`  |  Stoppt derzeitige Umfrage und gibt das Ergebnis aus. |  `>pollend`
`>pick`  |  Nimmt einen in diesem Channel hinterlegten Euro. |  `>pick`
`>plant`  |  Gib einen Euro aus, um in in diesen Channel zu legen. (Wenn der Bot neustartet, oder crashed, ist der Euro verloren) |  `>plant`
`>gencurrency`, `>gc`  |  Ändert Währungs Erstellung in diesem Channel. Jede geschriebene Nachricht hat eine Chance von 2%, einen Euro zu spawnen. Optionaler Parameter ist die Cooldown Zeit in Minuten. 5 Minuten sind Standard. Benötigt Manage Messages Berechtigungen |  `>gc` oder `>gc 60`
`>leet`  |  Konvertiert einen Text zu Leetspeak mit 6 (1-6) Stärke-Graden. |  `>leet 3 Hallo`
`>choose`  |  Sucht eine Sache aus einer Liste von Sachen aus. |  `>choose Get up;Sleep;Sleep more`
`>helix`  |  Stell dem allmächtigen Helix Fossil eine Ja/Nein Frage. |  `>helix should i do something`
`>rps`  |  Spiel eine Runde Stein, Schere, Papier mit . |  `>rps scissors`
`>linux`  |  Prints a customizable Linux interjection. |  `{Prefix}linux Spyware Windows`

### Level  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`!rank`  |  Zeigt deinen zurzeitigen Rang an.
`!ranks`, `!levels`  |  Schickt eine Rangliste per PN.
`!addxp`  |  Addet XP zu einem User
`!removexp`  |  Entfernt XP von einem User
`!turnToXP`  |  Tauscht Euro in XP um. Ratio 1/5

### Music  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`!next`, `!n`, `!skip`  |  Goes to the next song in the queue. You have to be in the same voice channel as the bot. |  `!n`
`!stop`, `!s`  |  Stops the music and clears the playlist. Stays in the channel. |  `!s`
`!destroy`, `!d`  |  Completely stops the music and unbinds the bot from the channel. (may cause weird behaviour) |  `!d`
`!pause`, `!p`  |  Pauses or Unpauses the song. |  `!p`
`!queue`, `!q`, `!yq`  |  Queue a song using keywords or a link. Bot will join your voice channel.**You must be in a voice channel**. |  `!q Dream Of Venice`
`!soundcloudqueue`, `!sq`  |  Queue a soundcloud song using keywords. Bot will join your voice channel.**You must be in a voice channel**. |  `!sq Dream Of Venice`
`!listqueue`, `!lq`  |  Lists 15 currently queued songs per page. Default page is 1. |  `!lq` or `!lq 2`
`!nowplaying`, `!np`  |  Shows the song currently playing. |  `!np`
`!volume`, `!vol`  |  Sets the music volume 0-100% |  `!vol 50`
`!defvol`, `!dv`  |  Sets the default music volume when music playback is started (0-100). Persists through restarts. |  `!dv 80`
`!mute`, `!min`  |  Sets the music volume to 0% |  `!min`
`!max`  |  Sets the music volume to 100%. |  `!max`
`!half`  |  Sets the music volume to 50%. |  `!half`
`!shuffle`, `!sh`  |  Shuffles the current playlist. |  `!sh`
`!playlist`, `!pl`  |  Queues up to 500 songs from a youtube playlist specified by a link, or keywords. |  `!pl playlist link or name`
`!soundcloudpl`, `!scpl`  |  Queue a soundcloud playlist using a link. |  `!scpl https://soundcloud.com/saratology/sets/symphony`
`!localplaylst`, `!lopl`  |  Queues all songs from a directory. **Bot Owner Only!** |  `!lopl C:/music/classical`
`!radio`, `!ra`  |  Queues a radio stream from a link. It can be a direct mp3 radio stream, .m3u, .pls .asx or .xspf (Usage Video: <https://streamable.com/al54>) |  `!ra radio link here`
`!local`, `!lo`  |  Queues a local file by specifying a full path. **Bot Owner Only!** |  `!lo C:/music/mysong.mp3`
`!move`, `!mv`  |  Moves the bot to your voice channel. (works only if music is already playing) |  `!mv`
`!remove`, `!rm`  |  Remove a song by its # in the queue, or 'all' to remove whole queue. |  `!rm 5`
`!movesong`, `!ms`  |  Moves a song from one position to another. |  `! ms` 5>3
`!setmaxqueue`, `!smq`  |  Sets a maximum queue size. Supply 0 or no argument to have no limit.  |  `!smq` 50 or `!smq`
`!cleanup`  |  Cleans up hanging voice connections. **Bot Owner Only!** |  `!cleanup`
`!reptcursong`, `!rcs`  |  Toggles repeat of current song. |  `!rcs`
`!rpeatplaylst`, `!rpl`  |  Toggles repeat of all songs in the queue (every song that finishes is added to the end of the queue). |  `!rpl`
`!save`  |  Saves a playlist under a certain name. Name must be no longer than 20 characters and mustn't contain dashes. |  `!save classical1`
`!load`  |  Loads a playlist under a certain name.  |  `!load classical-1`
`!playlists`, `!pls`  |  Lists all playlists. Paginated. 20 per page. Default page is 0. | `!pls 1`
`!deleteplaylist`, `!delpls`  |  Deletes a saved playlist. Only if you made it or if you are the bot owner. |  `!delpls animu-5`
`!goto`  |  Goes to a specific time in seconds in a song. |  `!goto 30`
`!getlink`, `!gl`  |  Shows a link to the song in the queue by index, or the currently playing song by default.
`!autoplay`, `!ap`  |  Toggles autoplay - When the song is finished, automatically queue a related youtube song. (Works only for youtube songs and when queue is empty)

### Searches  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`~lolchamp`  |  Überprüft die Statistiken eines LOL Champions. Bei Leerzeichen zusammenschreiben. Optionale Rolle. | ~lolchamp Riven or ~lolchamp Annie sup
`~lolban`  |  Zeigt die Top 12 gebannten LOL Champs. Banne diese 5 Champions und du wirst in kürzester Zeit Plat5 sein.
`~hitbox`, `~hb`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  `~hitbox SomeStreamer`
`~twitch`, `~tw`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  `~twitch SomeStreamer`
`~beam`, `~bm`  |  Benachrichtigt diesen Channek wenn ein bestimmter User anfängt zu streamen. |  `~beam SomeStreamer`
`~checkhitbox`, `~chhb`  |  Checkt ob ein bestimmter User auf gerade auf Hitbox streamt. |  `~chhb SomeStreamer`
`~checktwitch`, `~chtw`  |  Checkt ob ein bestimmter User auf gerade auf Twitch streamt. |  `~chtw SomeStreamer`
`~checkbeam`, `~chbm`  |  Checkt ob ein bestimmter User auf gerade auf Beam streamt. |  `~chbm SomeStreamer`
`~removestream`, `~rms`  |  Entfernt Benachrichtigung eines bestimmten Streamers auf diesem Channel. |  `~rms SomeGuy`
`~liststreams`, `~ls`  |  Listet alle Streams die du auf diesem Server folgst. |  `~ls`
`~convert`  |  Konvertiert Einheiten von>zu. |  `~convert m>km 1000`
`~convertlist`  |  Liste der kovertierbaren Dimensionen und Währungen.
`~wowjoke`  |  Get one of Kwoth's penultimate WoW jokes. |  `~wowjoke`
`~calculate`, `~calc`  |  Berechnet eine mathematische Angabe. |  `~calc 1+1`
`~calclist`  |  List operations of parser. |  `~calclist`
`~osu`  |  Zeigt Osu-Stats für einen Spieler. |  `~osu Name` oder `~osu Name taiko`
`~osu b`  |  Zeigt Informationen über eine Beatmap. |  `~osu b https://osu.ppy.sh/s/127712`
`~osu top5`  |  Zeigt die Top 5 Spiele eines Benutzers.  |  `~osu top5 Name`
`~pokemon`, `~poke`  |  Sucht nach einem Pokemon. |  `~poke Sylveon`
`~pokemonability`, `~pokeab`  |  Sucht nach einer Pokemon Fähigkeit. |  `~pokeab "water gun"`
`~randomcat`, `~meow`  |  Queries http://www.random.cat/meow.
`~i`, `~image`  |  Queries .
`~memelist`  |  Zeigt eine Liste von Memes, die du mit `~memegen` benutzen kannst, von http://memegen.link/templates/ |  `~memelist`
`~memegen`  |  Erstellt ein Meme von Memelist mit Top und Bottom Text. |  `~memegen biw "gets iced coffee" "in the winter"`
`~we`  |  Zeigt Wetter-Daten für eine genannte Stadt und ein Land. BEIDES IST BENÖTIGT. Wetter Api ist sehr zufällig, wenn du einen Fehler machst. |  `~we Moskau RF`
`~yt`  |  Durchsucht Youtube und zeigt das erste Ergebnis. |  `~yt query`
`~ani`, `~anime`, `~aq`  |  Durchsucht anilist nach einem Anime und zeigt das erste Ergebnis. |  `~aq aquerion evol`
`~imdb`  |  Durchsucht IMDB nach Filmen oder Serien und zeigt erstes Ergebnis. |  `~imdb query`
`~mang`, `~manga`, `~mq`  |  Durchsucht anilist nach einem Manga und zeigt das erste Ergebnis. |  `~mq query`
`~randomcat`, `~meow`  |  Zeigt ein zufälliges Katzenbild. |  `~meow`
`~randomdog`, `~woof`  |  Zeigt ein zufälliges Hundebild. |  `~woof`
`~i`  |  Zeigt das erste Ergebnis für eine Suche. Benutze ~ir für unterschiedliche Ergebnisse. |  `~i cute kitten`
`~ir`  |  Zeigt ein zufälliges Bild bei einem angegeben Suchwort. |  `~ir cute kitten`
`~lmgtfy`  |  Google etwas für einen Idioten. |  `~lmgtfy query`
`~google`, `~g`  |  Gibt einen Google-Suchlink für einen Begriff zurück. |  `~google query`
`~hs`  |  Sucht eine Heartstone-Karte und zeigt ihr Bild. Braucht eine Weile zum beenden. |  `~hs Ysera`
`~ud`  |  Durchsucht das Urban Dictionary nach einem Wort. |  `~ud Pineapple`
`~#`  |  Durchsucht Tagdef.com nach einem Hashtag. |  `~# ff`
`~quote`  |  Zeigt ein zufälliges Zitat. |  `~quote`
`~catfact`  |  Zeigt einen zufälligen Katzenfakt von <http://catfacts-api.appspot.com/api/facts> |  `~catfact`
`~yomama`, `~ym`  |  Zeigt einen zufälligen Witz von <http://api.yomomma.info/> |  `~ym`
`~randjoke`, `~rj`  |  Zeigt einen zufälligen Witz von <http://tambal.azurewebsites.net/joke/random> |  `~rj`
`~chucknorris`, `~cn`  |  Zeigt einen zufälligen Chuck Norris Witz von <http://tambal.azurewebsites.net/joke/random> |  `~cn`
`~stardew`  |  Gibt einen Link zum Stardew Valley Wiki mit gegebenem Topic zurück. |  `~stardew Cow`
`~magicitem`, `~mi`  |  Zeigt ein zufälliges Magic-Item von <https://1d4chan.org/wiki/List_of_/tg/%27s_magic_items> |  `~mi`
`~revav`  |  Gibt ein Google Reverse Image Search für das Profilbild einer Person zurück. |  `~revav "@SomeGuy"`
`~revimg`  |  Gibt eine 'Google Reverse Image Search' für ein Bild von einem Link zurück. |  `~revav Image link`
`~safebooru`  |  Zeigt ein zufälliges Hentai Bild von safebooru  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  `~safebooru yuri +kissing`
`~pony`, `~broni`  |  Shows a random image from bronibooru with a given tag. Tag is optional but preferred. (multiple tags are appended with +) |  `~pony scootaloo`
`~wiki`  |  Gibt einen Wikipedia-Link zurück.
`~clr`  |  Zeigt dir die zum Hex zugehörige Farbe.
 |  `~clr 00ff00`
`~videocall`  |  Erstellt einen privaten <http://www.appear.in> Video Anruf Link für dich und andere erwähnte Personen. Der Link wird allen erwähnten Personen per persönlicher Nachricht geschickt. |  `~videocall "@SomeGuy"`
`~av`, `~avatar`  |  Zeigt den Avatar einer erwähnten Person. |  `~av @X`

### Extra  
Befehl und Alternativen |  Beschreibung |  Benutzung
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
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`>active`  |  Zeigt das aktive Pokemon von jemandem oder einem selbst |  `>active` oder `>active @Someone`
`>pokehelp`, `>ph`  |  Zeigt die Basis Hilfe für Pokemon Kämpfe |  `>pokehelp`
`>movelist`, `>ml`  |  Zeigt eine Liste der verfügbaren Angriffe. |  `>movelist` oder `>ml` oder `>ml charmander`
`>switch`  |  Setzt dein aktives Pokemon per Nickname |  `>switch mudkip`
`>allmoves`, `>am`  |  Sendet dir eine private Nachticht mit allen Attacken deiner Pokemon. |  `>allmoves` oder `>am`
`>list`  |  Gibt eine Liste deiner Pokemon (6) zurück (aktives Pokemon ist unterstrichen) |  `>list`
`>elite4`, `>e4`  |  Zeigt die 5 stärksten Pokemon. |  `>elite4`
`>heal`  |  Heilt dein angegebenes Pokemon (per Nicknamen) oder das aktive Pokemon der gegebenen Person. |  `>heal bulbasaur` oder `>heal @user` oder `>heal all`
`>rename`, `>rn`  |  Benennt dein aktives Pokemon um. |  `>rename dickbutt` oder `>rn Mittens`
`>reset`  |  Setzt deine Pokemon zurück. KANN NICHT RÜCKGÄNGIG GEMACHT WERDEN |  `>reset true`
`>catch`  |  Versucht das derzeitige wilde Pokemon zu fangen. Du musst das Pokemon angeben, welches du ersetzen willst. Kostet einen Euro |  `>catch MyMudkip`
`>attack`, `>`  |  Greift gegebenes Ziel mit gegebener Attacke an. |  `>attack hyperbeam @user oder, `>attack @user flame-charge, > sunny-day @user`

### Translator  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`~translate`, `~trans`  |  Übersetzt Text von>zu. Von der gegebenen Sprache in die Zielsprache. |  `~trans en>fr Hello`
`~translangs`  |  Listet die verfügbaren Sprachen zur Übersetzung. |  `~translangs` oder `~translangs language`

### Memes  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`~9gag`, `~9r`  |  Gets a random 9gag post
`feelsbadman`, `FeelsBadMan`  |  FeelsBadMan
`feelsgoodman`, `FeelsGoodMan`  |  FeelsGoodMan

### NSFW  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`~hentai`  |  Zeigt ein zufälliges NSFW Hentai Bild von gelbooru und danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  `~hentai yuri+kissing`
`~atfbooru`, `~atf`  |  Shows a random hentai image from atfbooru with a given tag. Tag is optional but preffered. (multiple tags are appended with +) |  `~atf yuri+kissing`
`~danbooru`  |  Zeigt ein zufälliges Hentai Bild von danbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  `~danbooru yuri+kissing`
`~r34`  |  Zeigt ein zufälliges Hentai Bild von rule34.paheal.net mit einem gegebenen Tag. |  `~r34 bacon`
`~gelbooru`  |  Zeigt ein zufälliges Hentai Bild von gelbooru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. (mehrere Tags mit + zwischen den Tags) |  `~gelbooru yuri+kissing`
`~rule34`  |  Zeigt ein zufälliges Hentai Bild von rule34.xx  mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  `~rule34 yuri+kissing`
`~e621`  |  Zeigt ein zufälliges Hentai Bild von e621.net mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze Leerzeichen für mehrere Tags. |  `~e621 yuri+kissing`
`~derpi`  |  Zeigt ein zufälliges Hentai Bild von derpiboo.ru mit einem gegebenen Tag. Ein Tag ist optional aber bevorzugt. Benutze + für mehrere Tags. |  `~derpi yuri+kissing`
`~boobs`  |  Erwachsenen Inhalt. |  `~boobs`
`~butts`, `~ass`, `~butt`  |  Erwachsenen Inhalt. |  `~butts` oder `~ass`

### Sounds  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`!patrick`  |  Hier ist Patrick!
`!rage`  |  Das ist jetzt nicht euer Ernst!
`!airhorn`  |  Airhorn!
`!johncena`, `!cena`  |  JOHN CENA!
`!e`, `!ethanbradberry`, `!h3h3`  |  Ethanbradberry!
`!anotha`, `!anothaone`  |  Anothaone

### ClashOfClans  
Befehl und Alternativen |  Beschreibung |  Benutzung
----------------|--------------|-------
`,createwar`, `,cw`  |  Creates a new war by specifying a size (>10 and multiple of 5) and enemy clan name. | ,cw 15 The Enemy Clan
`,startwar`, `,sw`  |  Starts a war with a given number.
`,listwar`, `,lw`  |  Shows the active war claims by a number. Shows all wars in a short way if no number is specified. |  ,lw [war_number] or ,lw
`,claim`, `,call`, `,c`  |  Claims a certain base from a certain war. You can supply a name in the third optional argument to claim in someone else's place.  |  ,call [war_number] [base_number] [optional_other_name]
`,claimfinish`, `,cf`, `,cf3`, `,claimfinish3`  |  Finish your claim with 3 stars if you destroyed a base. Optional second argument finishes for someone else. |  ,cf [war_number] [optional_other_name]
`,claimfinish2`, `,cf2`  |  Finish your claim with 2 stars if you destroyed a base. Optional second argument finishes for someone else. |  ,cf [war_number] [optional_other_name]
`,claimfinish1`, `,cf1`  |  Finish your claim with 1 stars if you destroyed a base. Optional second argument finishes for someone else. |  ,cf [war_number] [optional_other_name]
`,unclaim`, `,uncall`, `,uc`  |  Removes your claim from a certain war. Optional second argument denotes a person in whose place to unclaim |  ,uc [war_number] [optional_other_name]
`,endwar`, `,ew`  |  Ends the war with a given index. | ,ew [war_number]

### Customreactions  
Befehl und Alternativen |  Beschreibung |  Benutzung
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
`Ich sage hallo zu dir`  |  Custom Reaction. | Ich sage hallo zu dir
