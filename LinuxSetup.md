#SETTING UP MidnightBot ON LINUX UBUNTU 14+

######If you want MidnightBot to play music for you 24/7 without having to hosting it on your PC and want to keep it cheap, reliable and convenient as possible, you can try MidnightBot on Linux Digital Ocean Droplet using the link http://m.do.co/c/46b4d3d44795/ (and using this link will be supporting MidnightBot and will give you **$10 credit**)

######Keep this helpful video handy https://www.youtube.com/watch?v=icV4_WPqPQk&feature=youtu.be (thanks to klincheR) it contains how to set up the Digital Ocean droplet aswell.


Assuming you have followed the link above to created an account in Digital Ocean and video to set up the bot until you get the `IP address and root password (in email)` to login, its time to begin:

**DOWNLOAD PuTTY**

http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html

**DOWNLOAD and INSTALL CyberDuck** `(for accessing filesystem using SFTP)`

https://cyberduck.io



**Follow the steps below:**

**Open PuTTY.exe** that you downloaded before, and paste or enter your `IP address` and then click **Open**

If you entered your Droplets IP address correctly, it should show **login as:** in a newly opened window.

Now for **login as:**, type `root` and hit enter.

It should then, ask for password, type the `root password` you have received in your **email address registered with Digital Ocean**, then hit Enter

*(as you are running it for the first time, it will most likely to ask you to change your root password, for that, type the "password you received through email", hit Enter, enter a "new password", hit Enter and confirm that "new password" again.*
**SAVE that new password somewhere safe not just in mind**

After you done that, you are ready to write commands.

**Copy and just paste** using **mouse right-click** (it should paste automatically)

######MONO (Source: http://www.mono-project.com/docs/getting-started/install/linux/)

**1)**

<pre><code class="language-bash">sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
</code></pre>
Note if the command is not be initiated, hit **Enter**

**2)**
<pre><code class="language-bash">echo "deb http://download.mono-project.com/repo/debian wheezy-apache24-compat main" | sudo tee -a /etc/apt/sources.list.d/mono-xamarin.list
</code></pre>

**3)**
<pre><code class="language-bash">apt-get install mono-devel
</code></pre>
**Type** `y` **hit Enter**
######Opus Voice Codec

**4)**
<pre><code class="language-bash">sudo apt-get install libopus0 opus-tools
</code></pre>
**Type** `y` **hit Enter**

**5)**
<pre><code class="language-bash">sudo apt-get install libopus-dev
</code></pre>

######FFMPEG

**6)**
<pre><code class="language-bash">apt-get install ffmpeg
</code></pre>
**Type** `y` **hit Enter**

######Uncomplicated Firewall UFW

**7)**
<pre><code class="language-bash">apt-get install ufw
</code></pre>
**it is most likely to have it already installed so if you see it is already installed, check with following command, and/or enable it**

**8)**
<pre><code class="language-bash">ufw status
</code></pre>

**9)**
<pre><code class="language-bash">ufw enable
</code></pre>
**Type** `y` **hit Enter**

**10)**
<pre><code class="language-bash">sudo ufw allow ssh
</code></pre>

######Unzip

**11)**
<pre><code class="language-bash">apt-get install unzip
</code></pre>

######TMUX
**12)**
<pre><code class="language-bash">apt-get install tmux
</code></pre>
**Type** `y` **hit Enter**

######NOW WE NEED TO IMPORT SOME DISCORD CERTS
**13)**
<pre><code class="language-bash">mozroots --import --ask-remove --machine
</code></pre>

**14)**
<pre><code class="language-bash">certmgr --ssl https://gateway.discord.gg
</code></pre>

Type `yes` and hit Enter **three times**


**15)**

Create a new folder “midnightbot” or anything you prefer
<pre><code class="language-bash">mkdir midnightbot
</code></pre>

**16)**

Move to “midnightbot” folder (note `cd --` to go back the directory)
<pre><code class="language-bash">cd midnightbot
</code></pre>

**NOW WE NEED TO GET MidnightBot FROM RELEASES**


Go to this link: https://github.com/Midnight-Myth/MidnightBotBot/releases and **copy the zip file address** of the lalest version available,

it should look like `https://github.com/Midnight-Myth/MidnightBot/releases/download/vx.x/MidnightBot.zip`

**17)**

Get the correct link, type `wget`, then *paste the link*, then hit **Enter**.
<pre><code class="language-bash">wget https://github.com/Midnight-Myth/MidnightBot/releases/download/vx.x/MidnightBot.zip
</code></pre>
**^Do not copy-paste it**

**18)**

Now we need to `unzip` the downloaded zip file and to do that, type the file name as it showed in your screen or just copy from the screen, should be like ` MidnightBot.zip`
<pre><code class="language-bash">unzip MidnightBot.zip
</code></pre>
**^Do not copy-paste it**

######NOW TO SETUP NADEKO

Open **CyberDuck**

Click on **Open Connection** (top-left corner), a new window should appear.

You should see **FTP (File Transfer Protocol)** in drop-down.

Change it to **SFTP (SSH File Transfer Protocol)**

Now, in **Server:** paste or type in your `Digital Ocean Droplets IP address`, leave `Port: 22` (no need to change it)

In **Username:** type `root`

In **Password:** type `the new root password (you changed at the start)`

Click on **Connect**

It should show you the new folder you created.

Open it.

######MAKE SURE YOU READ THE README BEFORE PROCEEDING

Copy the `credentials_example.json` to desktop

EDIT it as it is guided here: https://github.com/Midnight-Myth/MidnightBot/blob/master/README.md

Rename it to `credentials.json` and paste/put it back in the folder. `(Yes, using CyberDuck)`

You should see two files `credentials_example.json` and `credentials.json`

Also if you already have midnightbot setup and have `credentials.json`, `config.json`, `nadekobot.sqlite`, and `"permissions" folder`, you can just copy and paste it to the Droplets folder using CyberDuck.

######TIME TO RUN

Go back to **PuTTY**, `(hope its still running xD)`

**19)**

Type/ Copy and hit **Enter**.
<pre><code class="language-bash">tmux new -s midnightbot
</code></pre>
**^this will create a new session named “midnightbot”** `(you can replace “midnightbot” with anything you prefer and remember its your session name) so you can run the bot in background without having to keep running PuTTY in the background.`


<pre><code class="language-bash">cd midnightbot
</code></pre>

**20)**

<pre><code class="language-bash">mono MidnightBot.exe
</code></pre>

**CHECK THE BOT IN DISCORD, IF EVERYTHING IS WORKING**

Now time to **move bot to background** and to do that, press **CTRL+B+D** (this will ditach the midnightbot session using TMUX), and you can finally close PuTTY now.

And if you want to **see the sessions** after logging back again, type `tmux ls`, and that will give you the list of sessions running.

And if you want to **switch to/ see that session**, type `tmux a -t midnightbot` (**midnightbot** is the name of the session we created before so, replace **“midnightbot”** with the session name you created.)

**21)**

And if you want to **kill** MidnightBot **session**, type `tmux kill-session -t midnightbot`

######TO RESTART YOUR BOT ALONG WITH THE WHOLE SERVER (for science)
**22)**

Open **PuTTY** and login as you have before, type `reboot` and hit Enter.

######IF YOU WANT TO UPDATE YOUR BOT

**FOLLOW # STEPS SERIALLY**

**-21 OR 22**

**-19**

**-16**

**-17**

**-18**

**-20**

HIT **CTRL+B+D** and close **PuTTY**


`IF YOU FACE ANY TROUBLE ANYWHERE IN THE GUIDE JUST FIND US IN MidnightBot'S DISCORD SERVER`
