using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;

namespace Crowtails
{
    public partial class frmMain : Form
    {
        public string _mygroup = "";
        public string _myclass = "";

        SQLiteConnection m_dbConnection_live = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\dps_release.db;Version=3;");

        public List<string> sqlQueue = new List<string>();

        string cID = "";
        string cID_selected = "";
        DateTime lastD = DateTime.Now;

        DateTime lastAction = DateTime.Now;

        int lastNetID = 0;
        bool allhidden = false;
        int ShowDetailsNo = 0;
        string classdetect = "";
        int moa = 0;
        string fromuser = "";
        string lastsend = "";
        List<Panel> myPanels = new List<Panel>();
        public int movX;
        public int movY;
        int Mx;
        int My;
        bool isMoving;
        bool isSizing;

        public string _myaccount = "";

        int readyTimes = 0;

        HttpClient CliOn = new HttpClient();
        public string thatclass = "";

        int tutpage = 0;

        int setRdy = 0;
        int lastNetwork = 0;

        List<string> sqlSS = new List<string>();

        string lastLastInsertUnix = "";
        string ab = "";

        DateTime lastNetworkDT;
        bool firstnetwork = false;

        List<string> NetQue = new List<string>();

        int testlast = 0;
        bool isCollided;
        public List<string> NetworkQueue = new List<string>();

        SQLiteConnection m_dbConnection_readonly = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\dps_release.db;Mode=ReadOnly;Version=3;Read Only=True");


        public frmMain()
        {
            createDB();
            InitializeComponent();


            panel1.BringToFront();
            panel1.Dock = DockStyle.Fill;
            panel1.Visible = true;

            try
            {
                //myPanels.Add(main_p1);
                myPanels.Add(Details);
                myPanels.Add(Fights);
                // myPanels.Add(Enemys);
                myPanels.Add(DPSoutGraph);
                myPanels.Add(DPSinGraph);
                myPanels.Add(HPSoutGraph);
                myPanels.Add(SettingsWindow);
                myPanels.Add(DPSoutSkills);
                myPanels.Add(RaidDPSout);
                //myPanels.Add(RaidHPSout);

            }
            catch (Exception ex) { }

            try
            {
                this.DoubleBuffered = true;

                if (!Properties.Settings.Default.OverlayDis_)
                {
                    FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;  // no borders

                    TopMost = true;        // make the form always on top                     
                    Visible = true;        // Important! if this isn't set, then the form is not shown at all

                    this.WindowState = FormWindowState.Maximized;
                }
                if (Properties.Settings.Default.OverlayDis_)
                {
                    this.BackColor = Color.LightGray;
                }
            }
            catch (Exception ex) { }
            pictureBox4.Left = Width / 2 - 90;
            pictureBox4.Top = Height / 2 - 50;

            label37.Left = Width / 2 - 90;
            label37.Top = Height / 2 - 90;

        }

        private void btn_LiveParse_Click(object sender, EventArgs e)
        {
            SettingsWindow.Visible = false;
            bgw_wait4file.RunWorkerAsync();
            ((Button)sender).Enabled = false;
        }

        private void btn_OldParse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "Locallow") + @"\Art+Craft\Crowfall\CombatLogs\";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                if (btnJoinRaid.Text.Equals("leave")) {
                    btnJoinRaid.PerformClick();
                }

                tme_live_view.Enabled = false;
                btn_OldParse.Enabled = false;
                bgwParseOld.RunWorkerAsync(fd.FileName);

            }
        }

        private void bgw_live_DoWork(object sender, DoWorkEventArgs e)
        {
            using (FileStream stream = File.Open(e.Argument.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    try
                    {
                        reader.ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                    }

                    while (1 == 1)
                    {
                        try
                        {
                            var line = reader.ReadLine();
                            if (line != null)
                            {
                                bgw_live.ReportProgress(10, line);
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }

                        }
                        catch (Exception ex) { }
                    }
                }
            }
        }

        private void bgw_live_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                string thisline = parseLine(e.UserState.ToString(), _myclass, _myaccount, _mygroup);
                sqlQueue.Add(thisline);
            }
            catch (Exception ex)
            {
            }
        }

        class lcc
        {
            public string _cID { get; set; }
            public string caster { get; set; }
            public DateTime dateTime { get; set; }
            public bool self { get; set; }
            public bool dmg { get; set; }
            public bool heal { get; set; }
            public bool restore { get; set; }
            public bool crit { get; set; }
            public string target { get; set; }
            public string action { get; set; }
            public string amount { get; set; }
            public string skill { get; set; }
        }

        public void createDB()
        {

            SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\dps_release.db;Version=3;");
            string sql = "create table stats (id INTEGER PRIMARY KEY AUTOINCREMENT, fromuser varchar(50),  charclass varchar(5),caster varchar(50) ,skill varchar(100), cID varchar(20), dateTime varchar(25),self bool,dmg bool,heal bool,restore bool,crit bool,target varchar(50), action varchar(50), amount int)";
            try
            {
                m_dbConnection.Open();
            }
            catch (Exception ex) { }
            try
            {
                SQLiteCommand cmd = new SQLiteCommand(sql, m_dbConnection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { }

            m_dbConnection.Close();
        }

        private void bgw_wait4file_DoWork(object sender, DoWorkEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "Locallow") + @"\Art+Craft\Crowfall\CombatLogs\");
            FileInfo[] fi = di.GetFiles("*.log").OrderByDescending(p => p.CreationTime).ToArray();

            foreach (FileInfo f in fi)
            {
                try
                {
                    f.MoveTo(f.FullName.Replace(".log", ".txt"));
                }
                catch (Exception ex) { }
            }
            while (1 == 1)
            {
                try
                {
                    fi = di.GetFiles("*.log").OrderByDescending(p => p.CreationTime).ToArray();

                    if (fi[0].Exists)
                    {
                        e.Result = fi[0].FullName;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void bgw_wait4file_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bgw_live.RunWorkerAsync(e.Result.ToString());
        }

        private string generateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string parseLine(string line, string myclass, string myaccount, string mygroup)
        {
            lastAction = DateTime.Now;
            string _s = line;

            Match match1 = null;
            Match match = null;
            match1 = new Regex(@"(.*) INFO    COMBAT    - Combat _\|\|_ Event=\[(" + myaccount + @"|\w*)(.*) (hit|healed|drained|whiffed|restored) for (\d*)(.*)\.\]").Match(_s);
            if (match1.Success)
            {
                _s = _s.Replace("for", " Dummy for");
            }

            match = new Regex(@"(.*) INFO    COMBAT    - Combat _\|\|_ Event=\[(Your|\w*)(.*) (hit|healed|drained|whiffed|restored) (You|.*) for (\d*)(.*)\.\]").Match(_s);

            if (match.Groups[6].Value.Equals(0)) { return ""; };

            if (Account_.Text.Length > 0)
            {
                _s = _s.Replace("Your", myaccount).Replace("You", myaccount);
                _s = _s.Replace("Dein", myaccount).Replace("Du", myaccount);
                _s = _s.Replace("Ваш", myaccount).Replace("Вы", myaccount);
            }
            else {
                return "";
            }

            /*
            #caster_title# = Your, Playername, NPC
            #power_title# 
            #effect_verb# = hit, healed, drained, restored
            #target_title# = You, Playername, NPC
            for 
            #applied_value# = x "hit points", "damage"
            #critical_string#. = Critical
            */

            if (_s.Contains("ERROR") && _s.Contains("Combat is trying to find a client adapter that does not exist for this entity")) {
                MessageBox.Show("Repair Crowfalls and contact crowfall support logfile failture\r\nCombat is trying to find a client adapter that does not exist for this entity");
            }

            if (Properties.Settings.Default.gameLang == 0)
            {
                _s = _s.Replace(" Minion", "");
                
                List<string> powercontains = new List<string>();
                foreach (string mys in assets.powers)
                {
                    if (_s.ToLower().Contains(mys.ToLower()) && mys.Length > 4)
                    {
                        powercontains.Add(mys);
                    }
                }
                List<string> npccontains = new List<string>();
                foreach (string mys in assets.npcs)
                {
                    if (_s.ToLower().Contains(mys.ToLower()) && mys.Length > 4)
                    {
                        npccontains.Add(mys);
                    }
                }
                string power = powercontains.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
                string npc = npccontains.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);

                try
                {
                    _s = _s.Replace(power, power.Replace(" ", "_"));
                }
                catch (Exception ex) { }
                try
                {
                    _s = _s.Replace(npc, npc.Replace(" ", "_"));
                }
                catch (Exception ex) { }
                
            }

            // Language Setting
            /*
            0 EN
            1 DE
            2 RU
            3 ES
            4 FR
            */

            switch (Properties.Settings.Default.gameLang)
            {
                case 1:
                    _s = _s.Replace("getroffen", "hit")
                        .Replace("für", "for")
                        .Replace("Kritischer", "critical")
                        .Replace("Schaden", "damage")
                        .Replace("Du", "You")
                        .Replace("Dein", "Your")
                        .Replace("wiederhergestellt", "restored")
                        .Replace("gepafft", "whiffed")
                        .Replace("erschöpft", "drained")
                        .Replace("geheilt", "healed");
                    break;
                case 2:
                    _s = _s.Replace("цель", "hit")
                        .Replace("на", "for")
                        .Replace("поражает", "hit")
                        .Replace("Критический удар", "critical")
                        .Replace("урон", "damage")
                        .Replace("Вы", "You")
                        .Replace("Ваш", "Your")
                        .Replace("восстанавливает", "restored")
                        .Replace("поражает", "whiffed")
                        .Replace("поглощает", "drained")
                        .Replace("исцеляет", "healed");
                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    break;
            }

            match = new Regex(@"(.*) INFO    COMBAT    - Combat _\|\|_ Event=\[(" + myaccount + @"|\w*)(.*) (hit|healed|drained|whiffed|restored) (" + myaccount + @"|.*) for (\d*)(.*)\.\]").Match(_s);

            if (match.Success)
            {
                DateTime curD = DateTime.Parse(_s.Substring(0, 24));

                lcc l = new lcc();

                l._cID = "@@CID@@";
                l.dateTime = curD;
                l.amount = match.Groups[6].Value;
                l.caster = match.Groups[2].Value.Replace("'", "");
                l.target = match.Groups[5].Value.Replace("'", "");
                l.skill = match.Groups[3].Value.Trim().Replace("'", "");
                l.action = match.Groups[7].Value.Replace("(Critical).", "").Replace("'", "");
                if (match.Groups[7].Value.ToLower().Contains("critical")) l.crit = true;

                switch (match.Groups[4].Value)
                {
                    case "restored":
                        l.restore = true;
                        break;
                    case "healed":
                        l.heal = true;
                        break;
                    case "drained":
                        l.dmg = true;
                        l.caster = "drained_skill";
                        break;
                    case "hit":
                        l.dmg = true;
                        break;
                    case "whiffed":
                        l.dmg = true;
                        break;
                }

                if (l.skill != null)
                {
                    if (btnJoinRaid.Text.Equals("leave"))
                    {
                        lastNetID++;
                        string values = myaccount + ";" + myclass + ";" + l.caster + ";" + l.skill + ";@@;00:0" + lastNetID.ToString() + ";" + booltobit(l.self) + ";" + booltobit(l.dmg) + ";" + booltobit(l.heal) + ";" + booltobit(l.restore) + ";" + booltobit(l.crit) + ";" + l.target + ";" + l.action + ";" + l.amount;
                        NetworkQueue.Add(values);
                        return "";
                    }
                    else {
                        string values = "'" + myaccount + "','" + myclass + "','" + l.caster + "'," + "'" + l.skill + "','@@CID@@','" + l.dateTime + "','" + l.self + "','" + l.dmg + "','" + l.heal + "','" + l.restore + "','" + l.crit + "','" + l.target + "','" + l.action + "','" + l.amount + "'";
                        return "insert into stats (fromuser ,charclass ,caster, skill, cID, dateTime, self, dmg, heal, restore, crit, target, action, amount)values(" + values + ")";
                    }



                }
            }
            return "";
        }

        public string booltobit(bool b)
        {
            if (b) { return "A"; }
            else { return "B"; }

        }
        public bool bittobool(string b)
        {
            if (b.Equals("1")) { return true; }
            else { return false; }

        }

        public Color GetRandomColor(int index)
        {
            List<Color> cl = new List<Color>();
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);
            cl.Add(Color.DarkSlateBlue);

            return cl[index];

        }

        private void tme_live_view_Tick(object sender, EventArgs e)
        {

            if (fromuser.Equals(""))
            {
                fromuser = Account_.Text;
            } 

            if (cID.Length > 1)
            {
                moa++;
                if (Properties.Settings.Default.ShowOnlyInFight)
                {
                    DateTime curD = DateTime.Now;
                    var diffInSeconds = (curD - lastD).TotalSeconds;
                    if (diffInSeconds > 15 && !allhidden && !SettingsWindow.Visible)
                    {
                        foreach (Panel p in myPanels)
                        {
                            p.Visible = false;
                        }
                        allhidden = true;
                    }
                    else if (diffInSeconds < 15 && allhidden)
                    {
                        foreach (Panel p in myPanels)
                        {
                            if (!p.Name.Equals("SettingsWindow"))
                            {
                                p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);
                            }
                        }

                        allhidden = false;
                    }
                }
                DateTime curD1 = DateTime.Now;
                var diffInSeconds1 = (curD1 - lastAction).TotalSeconds;

                if (diffInSeconds1 < 180 || moa > 7)
                {
                    moa = 0;
                }

                if (diffInSeconds1 < 3)
                {

                    tme_live_view.Enabled = false;

                    SQLiteDataAdapter da1 = new SQLiteDataAdapter(
                      "select skill as Power, sum(amount) as SUM, count(*) as '#', sum(amount) /  count(*) as 'Ø', max(amount) as '»', IFNULL(SUM(CASE crit WHEN 'True' THEN 1 ELSE 0 END),0) as Crit,group_concat(DISTINCT  target) as Target from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and  dmg = 'True'  and amount > 0 group by skill order by sum(amount) desc;" +
                      "select skill as Power, sum(amount) as SUM, count(*) as '#', sum(amount) /  count(*) as 'Ø', max(amount) as '»',IFNULL(SUM(CASE crit WHEN 'True' THEN 1 ELSE 0 END),0)  as Crit,group_concat(DISTINCT  caster) as Caster  from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster not like '" + fromuser + "' and target = '" + fromuser + "' and  dmg = 'True'  and amount > 0 group by skill order by sum(amount) desc;" +
                      "select skill as Power, sum(amount) as SUM, count(*) as '#', sum(amount) /  count(*) as 'Ø', max(amount) as '»',IFNULL(SUM(CASE crit WHEN 'True' THEN 1 ELSE 0 END),0)  as Crit,group_concat(DISTINCT  target) as Target  from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and  heal = 'True'  and amount > 0 group by skill order by sum(amount) desc;" +
                      "select skill as Power, sum(amount) as SUM, count(*) as '#', sum(amount) /  count(*) as 'Ø', max(amount) as '»',IFNULL(SUM(CASE crit WHEN 'True' THEN 1 ELSE 0 END),0) as Crit,group_concat(DISTINCT  target) as Target  from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster not like '" + fromuser + "' and target = '" + fromuser + "' and  heal = 'True'  and amount > 0 group by skill order by sum(amount) desc;" +
                      "select sum(amount) || ' ' || action as Q from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and  dmg = 'True' and amount > 0 group by action order by sum(amount) desc;" +
                      "select sum(amount) || ' ' || action as Q from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster not like '" + fromuser + "' and target = '" + fromuser + "' and  dmg = 'True' and amount > 0 group by action order by sum(amount) desc;" +
                      "select max(id),sum(amount), REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and amount > 0 and dmg = 'True' group by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') order by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') asc;" +
                      "select max(id),sum(amount), REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and amount > 0 and heal = 'True' group by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') order by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') asc;" +
                      "select max(id),sum(amount), REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':',''), heal from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and ((caster not like '" + fromuser + "' and dmg = 'True') or heal = 'True') and Target = '" + fromuser + "' and amount > 0 group by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') order by REPLACE(REPLACE(REPLACE(datetime,'.',''),' ',''),':','') asc;" +
                      "select IFNULL(sum(amount),0) as TotalDMGout from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and dmg = 'True' and target not like '" + fromuser + "' and amount not like '0';" +
                      "select IFNULL(max(amount),0) as Maxhit from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and dmg = 'True' and target not like '" + fromuser + "' and amount not like '0';" +
                      "select IFNULL(sum(amount),0) as TotalHealout from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and heal = 'True' and amount not like '0';" +
                      "select IFNULL(max(amount),0) as Maxheal from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and heal = 'True' and amount not like '0';" +
                      "select count(*) as DMGHitCount from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and dmg = 'True' and target not like '" + fromuser + "' and amount not like '0';" +
                      "select count(*) as HealHitcount from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and heal = 'True' and amount not like '0';" +
                      "select IFNULL(sum(amount),0) as TotalHealin from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target = '" + fromuser + "' and heal = 'True' and amount not like '0';" +
                      "select IFNULL(sum(amount),0) as TotalDMGin from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target = '" + fromuser + "' and caster not like '" + fromuser + "' and dmg = 'True' and amount not like '0';" +
                      "select caster || '(' || sum(amount) || ')' as MostdangerEnemy from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target = '" + fromuser + "' and caster not like '" + fromuser + "' and dmg = 'True' and amount not like '0' group by target order by count(*) desc;" +
                      "select caster || '(' || count(*) || ')' as YourHealer from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and heal = 'True' and target = '" + fromuser + "' and amount not like '0' group by target order by count(*) desc;" +
                      "select IFNULL( target || '(' || sum(amount) || ')' , '') as YourTarget from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target not like '" + fromuser + "' and caster = '" + fromuser + "' and dmg = 'True' and amount not like '0' group by target order by count(*) desc;" +
                      "select count(*) as Dodges from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and 'action' like '%dodge%';" +
                      "select min(dateTime) as Start from stats where cID = '" + cID + "';" +
                      "select max(dateTime) as Ende from stats where cID = '" + cID + "';" +
                      "select count(*) as DMGCritOut from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and dmg = 'True' and crit = 'True' and amount not like '0' group by target order by count(*) desc;" +
                      "select count(*) as HealCritOut from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster = '" + fromuser + "' and heal = 'True' and crit = 'True' and amount not like '0' group by target order by count(*) desc;" +
                      "select count(*) as dmghitinc  from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target = '" + fromuser + "' and dmg = 'True' and amount not like '0';" +
                      "select count(*) as DMGCritIn from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster not like '" + fromuser + "' and target = '" + fromuser + "' and dmg = 'True' and crit = 'True' and amount not like '0' group by target order by count(*) desc;" +
                      "select count(*) as healhitinc from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and target = '" + fromuser + "' and heal = 'True' and amount not like '0';" +
                      "select count(*) as HealCritIn from stats where cID = '" + cID + "' and fromuser = '" + fromuser + "' and caster not like '" + fromuser + "' and target = '" + fromuser + "' and heal = 'True' and crit = 'True' and amount not like '0' group by target order by count(*) desc;" +

                      "select fromuser as Groupmember from stats where cID = '" + cID + "' group by fromuser;" +
                      "select target as Enemys from stats where cID = '" + cID + "' and fromuser not like target group by target;" +

                       "select a.fromuser, a.charclass , IFNULL(DMG,'0') as DMG,IFNULL(DPS,'0') as DPS, a.mi , a.ma , IFNULL(HEAL,'0') as HEAL,IFNULL(HPS,'0') as HPS,  b.mi , b.ma , IFNULL(DMGin,'0') as DMGin, IFNULL(HEALin,'0') as HEALin, '0' as Time, a.CH as DMGC, b.CH as DPSC, c.CH as DPSinC, d.CH as HPSC from (select MIN(dateTime) as mi, MAX(dateTime) as ma,SUM(amount) as DMG, '' as DPS, fromuser, cID, charclass, count(*) as CH from stats where  cID = '" + cID + "' and  dmg = 'True' and caster = fromuser group by fromuser,cID,charclass) as a " +
                      " left outer join (select MIN(dateTime) as mi, MAX(dateTime) as ma,SUM(amount) as HEAL, '' as HPS, fromuser, cID,charclass, count(*) as CH from stats where  cID = '" + cID + "' and  heal = 'True' and caster = fromuser group by fromuser,cID,charclass) as b on b.fromuser = a.fromuser" +
                      " left  outer join (select MIN(dateTime) as mi, MAX(dateTime) as ma,SUM(amount) as DMGin, '' as DPSin, fromuser, cID,charclass, count(*) as CH from stats where  cID = '" + cID + "' and  dmg = 'True' and target = fromuser and caster not like fromuser group by fromuser,cID,charclass) as c on c.fromuser = a.fromuser" +
                      " left  outer join (select MIN(dateTime) as mi, MAX(dateTime) as ma,SUM(amount) as HEALin, '' as HPSin, fromuser, cID,charclass, count(*) as CH from stats where  cID = '" + cID + "' and  heal = 'True' and target = fromuser and caster not like fromuser group by fromuser,cID,charclass) as d on d.fromuser = a.fromuser order by IFNULL(DMG,'0') desc;"

                      , m_dbConnection_readonly);
                    /* a.mi , a.ma , 
                     * MIN(dateTime) as mi, MAX(dateTime) as ma,
                    0  Detail 1 DMG 
                    1  Detail 2 DMGin 
                    2  Detail 3 Heal 
                    3  Detail 4 Healin 
                    4  DMG type out
                    5  DMG type in
                    6  DMG out Graph
                    7  Heal out Graph
                    8  Heal in DMG in Graph
                    9  TotalDMGout
                    10 Maxhit
                    11 TotalHealout
                    12 Maxheal
                    13 DMGHitCount
                    14 HealHitcount
                    15 TotalHealin
                    16 TotalDMGin
                    17 MostdangerEnemy
                    18 YourHealer
                    19 YourTarget
                    20 Dodges
                    21 Start 
                    22 Stop
                    23 DMGCritOut 
                    24 HealCritOut
                    25 dmghitinc 
                    26 DMGCritIn
                    27 healhitinc 
                    28 HealCritIn 
                    29 group user
                    30 ememys battle

                    31 RAID
                    32
                    33
                    34

                    35
                     */

                    DataTable fightID1 = new DataTable();
                    DataSet fDetail = new DataSet();

                    Stopwatch sw = new Stopwatch();
                    try
                    {
                        da1.Fill(fDetail);
                    }
                    catch (Exception ex)
                    {
                        tme_live_view.Enabled = true;
                        return;
                    }

                    DateTime start = DateTime.Now;
                    DateTime stop = DateTime.Now;
                    try
                    {
                        start = DateTime.Parse(fDetail.Tables[21].Rows[0][0].ToString());
                        stop = DateTime.Parse(fDetail.Tables[22].Rows[0][0].ToString());
                    }
                    catch (Exception ex) { }

                    var diffInSeconds = (stop - start).TotalSeconds;
                    TimeSpan elapsed = TimeSpan.FromSeconds(diffInSeconds);
                    string elapsedFormatted = elapsed.ToString(@"m\:ss");
                    Time.Text = elapsedFormatted.ToString();
                    dgDPSOut.DataSource = fDetail.Tables[31];

                    foreach (DataRow dr in fDetail.Tables[31].Rows)
                    {
                        try
                        {
                            dr[1] = cbClass.Items[Convert.ToInt32(dr[1])].ToString();
                        }
                        catch (Exception ex) { }

                        try
                        {
                            //(4/5)
                            var diffInSecondsrel = (DateTime.Parse(dr[4].ToString()) - DateTime.Parse(dr[5].ToString())).TotalSeconds - 1;
                            dr[3] = (Convert.ToDouble(dr[2]) / Convert.ToDouble(dr[13])).ToString("#");//Math.Abs(diffInSecondsrel)).ToString("#");
                            if (dr[3].Equals("")) { dr[3] = "0"; }
                        }
                        catch (Exception ex) { }
                        try
                        {
                            // (8/9)
                            var diffInSecondsrel = (DateTime.Parse(dr[8].ToString()) - DateTime.Parse(dr[9].ToString())).TotalSeconds - 1;
                            dr[7] = (Convert.ToDouble(dr[6]) /  Convert.ToDouble(dr[16])).ToString("#"); //Math.Abs(diffInSecondsrel)).ToString("#");
                            if (dr[7].Equals("")) { dr[7] = "0"; }
                        }
                        catch (Exception ex) { }

                        try
                        {
                            DateTime realstart = new DateTime();
                            DateTime realstop = new DateTime();

                            if (dr[8].ToString().Length > 1 && dr[4].ToString().Length > 1)
                            {
                                realstart = new DateTime(Math.Min(DateTime.Parse(dr[8].ToString()).Ticks, DateTime.Parse(dr[4].ToString()).Ticks));
                            }
                            if (dr[4].ToString().Length > 1 && dr[8].ToString().Length < 1)
                            {
                                realstart = DateTime.Parse(dr[4].ToString());
                            }
                            if (dr[4].ToString().Length < 1 && dr[8].ToString().Length > 1)
                            {
                                realstart = DateTime.Parse(dr[8].ToString());
                            }
                            if (dr[9].ToString().Length > 1 && dr[5].ToString().Length < 1)
                            {
                                realstop = DateTime.Parse(dr[9].ToString());
                            }
                            if (dr[9].ToString().Length < 1 && dr[5].ToString().Length > 1)
                            {
                                realstop = DateTime.Parse(dr[5].ToString());
                            }
                            if (dr[9].ToString().Length > 1 && dr[5].ToString().Length > 1)
                            {
                                realstop = new DateTime(Math.Max(DateTime.Parse(dr[9].ToString()).Ticks, DateTime.Parse(dr[5].ToString()).Ticks));
                            }

                            var realdiv = (realstop - realstart).TotalSeconds + 1;

                            TimeSpan elapsed12 = TimeSpan.FromSeconds(realdiv);
                            dr[12] = elapsed12.ToString(@"m\:ss");
                        }
                        catch (Exception ex) { }
                    }

                    lstInGroup.DisplayMember = "Groupmember";
                    lstInGroup.ValueMember = "Groupmember";
                    lstInGroup.DataSource = fDetail.Tables[29];
                    lstEnemys.DisplayMember = "Enemys";
                    lstEnemys.DataSource = fDetail.Tables[30];


                    sw.Start();

                    int ii = 0;
                    foreach (DataTable dt in fDetail.Tables)
                    {
                        if (ii > 3) { break; }
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr.ItemArray.Length > 1)
                            {
                                if (Convert.ToDouble(dr.ItemArray[5]) > 0 && Convert.ToDouble(dr.ItemArray[2]) > 0)
                                    try
                                    {
                                        dr[5] = Convert.ToInt64((Convert.ToDouble(dr.ItemArray[5]) / (Convert.ToDouble(dr.ItemArray[2]) / Convert.ToDouble(100))).ToString("#"));
                                    }
                                    catch (Exception ex) { }
                            }
                        }
                        ii++;
                    }

                    dgDetails.DataSource = fDetail.Tables[ShowDetailsNo];
                    foreach (DataGridViewColumn column in dgDetails.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }

                    fightID1 = fDetail.Tables[0];
                    int i = 0;

                    chart3.Series[0].Points.Clear();
                    chart3.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                    chart3.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
                    for (int abc = 0; abc < fightID1.Rows.Count;abc++) {
                        DataRow s = fightID1.Rows[abc];
                        chart3.Series[0].Points.AddXY(abc, Convert.ToDouble(abc), Convert.ToDouble(s[1]));
                        chart3.Series[0].Points[abc].Label = s[0].ToString() + " (" + s[1].ToString() + ")";
                    }

                    lstInType.DisplayMember = "Q";
                    lstInType.DataSource = fDetail.Tables[4];
                    lstOutType.DisplayMember = "Q";
                    lstOutType.DataSource = fDetail.Tables[5];

                    chart1.Series[0].Points.Clear();
                    DataTable fightID2 = fDetail.Tables[6];

                    chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                    chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

                    i = 0;
                    foreach (DataRow s in fightID2.Rows)
                    {
                        i++;
                        chart1.Series[0].Points.AddXY(Convert.ToDouble(s[2]), Convert.ToDouble(s[1]));
                    }

                    chart4.Series[0].Points.Clear();
                    fightID2 = fDetail.Tables[7];

                    chart4.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                    chart4.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

                    foreach (DataRow s in fightID2.Rows)
                    {
                        chart4.Series[0].Points.AddXY(Convert.ToDouble(s[2]), Convert.ToDouble(s[1]));
                    }

                    chart2.Series[0].Points.Clear();
                    chart2.Series[1].Points.Clear();
                    fightID2 = fDetail.Tables[8];

                    chart2.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                    chart2.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;

                    int ia = 0;
                    foreach (DataRow s in fightID2.Rows)
                    {
                        if (Convert.ToBoolean(s[3]) == false)
                        {
                            chart2.Series[0].Points.AddXY(Convert.ToDouble(ia), Convert.ToDouble(s[1]));
                        }
                        else
                        {
                            chart2.Series[1].Points.AddXY(Convert.ToDouble(ia), Convert.ToDouble(s[1]));
                        }
                        ia++;
                    }




                    try
                    {
                        Dodges.Text = fDetail.Tables[20].Rows[0][0].ToString();

                        Time.Text = elapsedFormatted.ToString();
                        time1.Text = elapsedFormatted.ToString();
                        time2.Text = elapsedFormatted.ToString();
                        time3.Text = elapsedFormatted.ToString();

                        double dpsd = (Convert.ToDouble(fDetail.Tables[9].Rows[0][0].ToString()) / diffInSeconds);
                        double dpsoutd = Convert.ToDouble(fDetail.Tables[9].Rows[0][0].ToString()) / diffInSeconds;
                        double hpsd = (Convert.ToDouble(fDetail.Tables[11].Rows[0][0].ToString()) / diffInSeconds);
                        double hpsoutd = (Convert.ToDouble(fDetail.Tables[11].Rows[0][0].ToString()) / diffInSeconds);

                        double dpsind = (Convert.ToDouble(fDetail.Tables[16].Rows[0][0].ToString()) / diffInSeconds);
                        double hpsind = (Convert.ToDouble(fDetail.Tables[15].Rows[0][0].ToString()) / diffInSeconds);

                        List<double> dl = new List<double>();
                        dl.Add(dpsd);
                        dl.Add(dpsoutd);
                        dl.Add(hpsd);
                        dl.Add(hpsoutd);
                        dl.Add(dpsind);
                        dl.Add(hpsind);

                        for (int iaa = 0; iaa < dl.Count; iaa++)
                        {
                            if (double.IsNaN(dl[iaa]) || dl[iaa] > 1)
                            {
                                dl[iaa] = 0;
                            }
                        }

                        DPS.Text = dpsd.ToString("#");
                        HPS.Text = hpsd.ToString("#");

                        BIGDMG.Text = fDetail.Tables[10].Rows[0][0].ToString();
                        BIGHEAL.Text = fDetail.Tables[12].Rows[0][0].ToString();
                        DMGsum.Text = fDetail.Tables[9].Rows[0][0].ToString();
                        HEALsum.Text = fDetail.Tables[11].Rows[0][0].ToString();
                        DMGHitCount.Text = fDetail.Tables[13].Rows[0][0].ToString();
                        HealHitCount.Text = fDetail.Tables[14].Rows[0][0].ToString();

                        DPSIn12.Text = dpsind.ToString("#");
                        HPSIn.Text = hpsind.ToString("#");

                        TotalDMGin.Text = fDetail.Tables[16].Rows[0][0].ToString();
                        TotalHealin.Text = fDetail.Tables[15].Rows[0][0].ToString();
                        try { YourTarget.Text = fDetail.Tables[19].Rows[0][0].ToString(); } catch (Exception ex) { }
                        try { MostdangerEnemy.Text = fDetail.Tables[17].Rows[0][0].ToString(); } catch (Exception ex) { }
                        try { YourHealer.Text = fDetail.Tables[18].Rows[0][0].ToString(); } catch (Exception ex) { }




                        DPSout1.Text = DMGsum.Text + " = " + DPS.Text + "/sec";
                        HPSout1.Text = HEALsum.Text + " = " + HPS.Text + "/sec";
                        label22.Text = "DMG in : " + TotalDMGin.Text + " = " + DPSIn12.Text + "/sec | HEAL in : " + TotalHealin.Text + " = " + HPSIn.Text + "/sec";
                    }
                    catch (Exception ex) { }


                    try
                    {
                        double am = (Convert.ToDouble(fDetail.Tables[23].Rows[0][0].ToString()) / (Convert.ToDouble(DMGHitCount.Text) / 100));
                        if (!double.IsNaN(am) && am > 1)
                        {
                            DMGCrit.Text = am.ToString("#") + "%";
                        }
                        else
                        {
                            DMGCrit.Text = "0";
                        }
                    }
                    catch (Exception ex) { }
                    try
                    {
                        double am = (Convert.ToDouble(fDetail.Tables[24].Rows[0][0].ToString()) / (Convert.ToDouble(HealHitCount.Text) / 100));
                        if (!double.IsNaN(am) && am > 1)
                        {
                            HealCrit.Text = am.ToString("#") + "%";
                        }
                        else
                        {
                            HealCrit.Text = "0";
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        double am = (Convert.ToDouble(fDetail.Tables[26].Rows[0][0].ToString()) / (Convert.ToDouble(fDetail.Tables[25].Rows[0][0].ToString()) / 100));
                        if (!double.IsNaN(am) && am > 1)
                        {
                            DMGCritIn.Text = am.ToString("#") + "%";
                        }
                        else
                        {
                            DMGCritIn.Text = "0";
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        double am = (Convert.ToDouble(fDetail.Tables[28].Rows[0][0].ToString()) / (Convert.ToDouble(fDetail.Tables[27].Rows[0][0].ToString()) / 100));
                        if (am != Double.NaN && am > 1)
                        {
                            HealCritIn.Text = am.ToString("#") + "%";
                        }
                        else
                        {
                            HealCritIn.Text = "0";
                        }

                    }
                    catch (Exception ex) { }

                    sw.Stop();
                    label4.Text = sw.ElapsedMilliseconds.ToString();
                    tme_live_view.Enabled = true;
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private void btn_get_old_Click(object sender, EventArgs e)
        {

            if (btnJoinRaid.Text.Equals("leave"))
            {
                btnJoinRaid.PerformClick();
            }

            dgFights.DataSource = null;
            dgFights.AutoGenerateColumns = false;
            SQLiteDataAdapter da = new SQLiteDataAdapter("select min(dateTime) as date, max(dateTime), cID as name from stats group by cID order by dateTime desc", m_dbConnection_readonly);
            DataTable fightID = new DataTable();
            try
            {
                da.Fill(fightID);
            }
            catch (Exception ex) {
                return;
            }

            fightID.Columns.Add("sec");

            foreach (DataRow dr in fightID.Rows)
            {
                DateTime start = DateTime.Parse(dr[0].ToString());
                DateTime stop = DateTime.Parse(dr[1].ToString());

                var diffInSeconds = (stop - start).TotalSeconds;
                if (diffInSeconds > 5)
                {

                    TimeSpan elapsed = TimeSpan.FromSeconds(diffInSeconds);
                    string elapsedFormatted = elapsed.ToString(@"m\:ss");

                    dr[3] = elapsedFormatted;
                }
                else
                {
                    dr.Delete();
                }
            }

            dgFights.DataSource = fightID;


            listBox2.Items.Clear();

            SQLiteDataAdapter da1 = new SQLiteDataAdapter("select Target || ' (' || count(*) || ')' from stats group by Target", m_dbConnection_readonly);
            DataTable fightTargets = new DataTable();
            da1.Fill(fightTargets);


            foreach (DataRow dr in fightTargets.Rows)
            {
                listBox2.Items.Insert(0, dr[0]);
            }
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            isMoving = true;
            movX = e.X;
            movY = e.Y;
            isCollided = false;
            try
            {
                ((Panel)sender).Parent.BringToFront();

                foreach (Panel p in myPanels)
                {
                    if (((Panel)sender).Parent != p)
                    {
                        if (((Panel)sender).Parent.Bounds.IntersectsWith(p.Bounds))
                        {
                            isCollided = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving && !checkBox13.Checked)
            {
                if (MousePosition.X < this.Left + (this.Width / 100 * 95) && MousePosition.X > this.Left)
                {
                    ((Panel)sender).Parent.Left = MousePosition.X - movX - this.Left;
                }
                if (MousePosition.Y < this.Top + (this.Height / 100 * 95) && MousePosition.Y > this.Top)
                {
                    ((Panel)sender).Parent.Top = MousePosition.Y - movY - this.Top;
                }

                bool curCol = false;
                foreach (Panel p in myPanels)
                {
                    if (((Panel)sender).Parent != p && p.Visible && Properties.Settings.Default.DockWindow)
                    {
                        if (((Panel)sender).Parent.Bounds.IntersectsWith(p.Bounds))
                        {
                            ((Panel)sender).Parent.Top = p.Top;
                            curCol = true;
                            break;
                        }
                    }
                }

                if (isCollided && !curCol)
                {
                    isCollided = false;
                }
                else if (curCol && !isCollided)
                {
                    isMoving = false;
                }
            }
        }

        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            string Prop = ((Panel)sender).Parent.Name + "Set";
            string[] oldSet = Properties.Settings.Default[Prop].ToString().Split(';');
            oldSet[0] = ((Panel)sender).Parent.Left.ToString();
            oldSet[1] = ((Panel)sender).Parent.Top.ToString();
            string newSet = "";
            foreach (string s in oldSet)
            {
                newSet += s + ";";
            }
            Properties.Settings.Default[Prop] = newSet;
            Properties.Settings.Default.Save();

            isMoving = false;
        }

        private void panel3h_MouseDown(object sender, MouseEventArgs e)
        {

            // Assign this method to mouse_Down event of Form or Panel,whatever you want
            isMoving = true;
            movX = e.X;
            movY = e.Y;
            isCollided = false;
            try
            {
                ((Label)sender).Parent.Parent.BringToFront();

                foreach (Panel p in myPanels)
                {
                    if (((Label)sender).Parent.Parent != p)
                    {
                        if (((Label)sender).Parent.Parent.Bounds.IntersectsWith(p.Bounds))
                        {
                            isCollided = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void panel3h_MouseMove(object sender, MouseEventArgs e)
        {

            // Assign this method to Mouse_Move event of that Form or Panel
            if (isMoving && !checkBox13.Checked)
            {

                if (MousePosition.X < this.Left + (this.Width / 100 * 95) && MousePosition.X > this.Left)
                {
                    ((Label)sender).Parent.Parent.Left = MousePosition.X - movX - this.Left;
                }
                if (MousePosition.Y < this.Top + (this.Height / 100 * 95) && MousePosition.Y > this.Top)
                {
                    ((Label)sender).Parent.Parent.Top = MousePosition.Y - movY - this.Top;
                }


                bool curCol = false;

                foreach (Panel p in myPanels)
                {
                    if (((Label)sender).Parent.Parent != p && p.Visible && Properties.Settings.Default.DockWindow)
                    {
                        if (((Label)sender).Parent.Parent.Bounds.IntersectsWith(p.Bounds))
                        {
                            ((Label)sender).Parent.Parent.Top = p.Top;
                            curCol = true;
                            break;
                        }
                    }
                }

                if (isCollided && !curCol)
                {
                    isCollided = false;
                }
                else if (curCol && !isCollided)
                {
                    isMoving = false;
                }
            }
        }

        private void panel3h_MouseUp(object sender, MouseEventArgs e)
        {
            string Prop = ((Label)sender).Parent.Parent.Name + "Set";
            string[] oldSet = Properties.Settings.Default[Prop].ToString().Split(';');
            oldSet[0] = ((Label)sender).Parent.Parent.Left.ToString();
            oldSet[1] = ((Label)sender).Parent.Parent.Top.ToString();
            string newSet = "";
            foreach (string s in oldSet)
            {
                newSet += s + ";";
            }
            Properties.Settings.Default[Prop] = newSet;
            Properties.Settings.Default.Save();

            isMoving = false;
        }

        private void panel3_MouseLeave(object sender, EventArgs e)
        {
            isMoving = false;
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            isSizing = true;
            movX = e.X;
            movY = e.Y;
            ((Panel)sender).Parent.BringToFront();
            SWidth = ((Panel)sender).Parent.Left;
            SHeight = ((Panel)sender).Parent.Top;
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            string Prop = ((Panel)sender).Parent.Name + "Set";
            string[] oldSet = Properties.Settings.Default[Prop].ToString().Split(';');
            oldSet[2] = ((Panel)sender).Parent.Width.ToString();
            oldSet[3] = ((Panel)sender).Parent.Height.ToString();
            string newSet = "";
            foreach (string s in oldSet)
            {
                newSet += s + ";";
            }
            Properties.Settings.Default[Prop] = newSet;
            Properties.Settings.Default.Save();

            isSizing = false;
        }

        public int SHeight;
        public int SWidth;
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSizing && !checkBox13.Checked)
            {
                this.Invalidate();
                ((Panel)sender).Parent.Width = MousePosition.X - SWidth - this.Left;
                ((Panel)sender).Parent.Height = MousePosition.Y - SHeight - this.Top;
            }
        }


        private void panel2_MouseLeave(object sender, EventArgs e)
        {
            isSizing = false;
        }

        private void lblDetails_Click(object sender, EventArgs e)
        {
            toggleDetailsMenu();
        }

        private void toggleDetailsMenu()
        {

            pnlDetailsMenu.Visible = !pnlDetailsMenu.Visible;
            pnlDetailsMenu.BringToFront();
            if (pnlDetailsMenu.Visible)
            {
                lblDetails.BackColor = Color.FromArgb(255, 255, 192);
                lblDetails.ForeColor = Color.FromArgb(0, 0, 0);
            }
            else
            {
                lblDetails.BackColor = Color.FromArgb(0, 0, 0);
                lblDetails.ForeColor = Color.FromArgb(255, 255, 255);
            }
        }

        private void cbOpacity_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Opacity_.SelectedIndex)
            {
                case 0:
                    this.Opacity = 0.95;
                    break;
                case 1:
                    this.Opacity = 0.9D;
                    break;
                case 2:
                    this.Opacity = 0.85;
                    break;
                case 3:
                    this.Opacity = 0.8D;
                    break;
                case 4:
                    this.Opacity = 0.75;
                    break;
                case 5:
                    this.Opacity = 0.7D;
                    break;
                default:
                    this.Opacity = 100;
                    break;

            }

            Properties.Settings.Default.Opacity_ = Opacity_.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default[((CheckBox)sender).Tag.ToString()] = ((CheckBox)sender).Checked;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            Boolean test = Convert.ToBoolean(Properties.Settings.Default[((CheckBox)sender).Tag.ToString()]);

            foreach (Panel p in myPanels)
            {
                if (p.Name.Equals(((CheckBox)sender).Tag.ToString()))
                {
                    p.Visible = ((CheckBox)sender).Checked;
                }
            }

            if (!checkBox11.Enabled)
            {

                foreach (Panel p in myPanels)
                {
                    p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);

                }

                allhidden = false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SettingsWindow.Visible = !SettingsWindow.Visible;

            Properties.Settings.Default.SettingsWindow = SettingsWindow.Visible;
            Properties.Settings.Default.Save();


            foreach (Panel p in myPanels)
            {
                if (!p.Name.Equals("SettingsWindow"))
                {
                    p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);
                }
            }

            allhidden = false;
        }

        private void btnResetWindows_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.Controls)
            {
                try
                {
                    if (c.GetType() == typeof(Panel))
                    {
                        Properties.Settings.Default[c.Name + "Set"] = "0;0;0;0;";
                    }
                }
                catch (Exception ex) { }
            }
            Properties.Settings.Default.Save();
        }

        private void Account__Leave(object sender, EventArgs e)
        {
            _myaccount = Account_.Text;
            fromuser = Account_.Text;
            Properties.Settings.Default.Account_ = Account_.Text;
            Properties.Settings.Default.Save();
        }

        private void ApiKey__SelectedIndexChanged(object sender, EventArgs e)
        {

            _mygroup = ApiKey_.SelectedIndex.ToString();
            Properties.Settings.Default.ApiKey_ = ApiKey_.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void btnDPSout_Click(object sender, EventArgs e)
        {
            lblDetails.Text = "DPS out";
            ShowDetailsNo = 0;
            toggleDetailsMenu();
            tme_live_view.Enabled = true;
            lastAction = DateTime.Now;
        }

        private void btnDPSin_Click(object sender, EventArgs e)
        {

            lblDetails.Text = "DPS in";
            ShowDetailsNo = 1;
            toggleDetailsMenu();
            tme_live_view.Enabled = true;
            lastAction = DateTime.Now;
        }

        private void btnHPSout_Click(object sender, EventArgs e)
        {

            lblDetails.Text = "HPS out";
            ShowDetailsNo = 2;
            toggleDetailsMenu();
            tme_live_view.Enabled = true;
            lastAction = DateTime.Now;
        }

        private void btnHPSin_Click(object sender, EventArgs e)
        {

            lblDetails.Text = "HPS in";
            ShowDetailsNo = 3;
            toggleDetailsMenu();
            tme_live_view.Enabled = true;
            lastAction = DateTime.Now;
        }

        private void btnConditions_Click(object sender, EventArgs e)
        {
            lblDetails.Text = "Conditions (not implemented yet)";
            toggleDetailsMenu();
            tme_live_view.Enabled = true;
            lastAction = DateTime.Now;
        }

        private void btnJoinRaid_Click(object sender, EventArgs e)
        {
            if (Account_.Text.Length > 1)
            {
                if (btnJoinRaid.Text.Equals("join"))
                {
                    btnJoinRaid.Text = "leave";
                    thisOnlinePnl.Visible = false;
                    button6.Visible = true;
                }
                else
                {
                    btnJoinRaid.Text = "join";
                    ApiKey_.Enabled = true;
                    thisOnlinePnl.Visible = true;
                    button6.Visible = false;
                }
            }
        }

        private void btnLeaveRaid_Click(object sender, EventArgs e)
        {
        }

        public void findUserName()
        {
            try
            {
                if (Account_.Text.Length < 2)
                {
                    DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "Locallow") + @"\Art+Craft\Crowfall\");
                    string txt = File.ReadAllText(di.GetFiles("CLIENT_*.log")[0].FullName);
                    int start = txt.IndexOf("Initialized local player _||_ Name=[");
                    int stop = txt.IndexOf("]", start);

                    string playername = txt.Substring(start + "Initialized local player _ || _ Name".Length, stop - (start + "Initialized local player _ || _ Name".Length));
                    Account_.Text = playername.Replace("Initialized local player _ || _ Name =[", "");
                    Properties.Settings.Default.Account_ = Account_.Text;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex) { }
        }

        private void FightDura__SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FightDura_ = Convert.ToInt32(FightDura_.Items[FightDura_.SelectedIndex]);
            Properties.Settings.Default.Save();
        }

        private void btnNewFight_Click(object sender, EventArgs e)
        {
            cID = generateID();
            cID_selected = cID;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnJoinRaid.Text.Equals("join"))
            {
                btnJoinRaid_Click(sender, e);
            }
        }

        private void ApiKey__Click(object sender, EventArgs e)
        {
            if (listBox3.Items.IndexOf(Account_.Text) > -1)
            {
                ApiKey_.Enabled = false;
            }
            else
            {
                ApiKey_.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SettingsWindow.Visible = true;
        }

        private void bgwParseOld_DoWork(object sender, DoWorkEventArgs e)
        {
            SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\dps_release.db;Version=3;");
            try
            {
                m_dbConnection.Open();
            }
            catch (Exception ex) { }

            string logfile = File.ReadAllText(e.Argument.ToString());
            SQLiteCommand cmd = new SQLiteCommand(m_dbConnection);
            cmd.CommandText = "BEGIN TRANSACTION;";
            cmd.ExecuteNonQuery();
            foreach (string s in logfile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    cmd.CommandText = parseLine(s, _myclass, _myaccount, _mygroup);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex) { }
            }
            cmd.CommandText = "END TRANSACTION;";
            cmd.ExecuteNonQuery();
            m_dbConnection.Close();
        }

        private void bgwParseOld_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btn_OldParse.Enabled = true;
            tme_live_view.Enabled = true;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            try {
                textBox2.Text = Properties.Settings.Default.ServerUrl;
            } catch (Exception ex) { }

            if (Properties.Settings.Default.StartCrowfall)
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Properties.Settings.Default.CrowfallFolder;
                    p.Start();
                }
                catch (Exception ex) { }
            }
            createDB();

            try
            {
                foreach (Control c in SettingsWindow.Controls)
                {
                    try
                    {
                        if (c.GetType() == typeof(CheckBox) && c.Tag != null)
                        {
                            ((CheckBox)c).Checked = Convert.ToBoolean(Properties.Settings.Default[c.Tag.ToString()]);
                        }
                    }
                    catch (Exception ex) { }
                }

            }
            catch (Exception ex) { }

            try
            {
                cbClass.SelectedIndex = Properties.Settings.Default.Class_;
                Account_.Text = Properties.Settings.Default.Account_;
                ApiKey_.SelectedIndex = Properties.Settings.Default.ApiKey_;

                uiLang.SelectedIndex = Properties.Settings.Default.uilang;
                gameLang.SelectedIndex = Properties.Settings.Default.gameLang;

                for (int i = 0; i < FightDura_.Items.Count; i++)
                {
                    if (FightDura_.Items[i].ToString().Equals(Properties.Settings.Default.FightDura_.ToString()))
                    {
                        FightDura_.SelectedIndex = i;
                    }
                }

                Opacity_.SelectedIndex = Properties.Settings.Default.Opacity_;
            }
            catch (Exception ex) { }


            findUserName();

            try
            {
                if (Account_.Text.Length > 2)
                {
                    Account_.Enabled = true;
                }
            }
            catch (Exception) { }

            try
            {
                this.Location = Screen.AllScreens[0].WorkingArea.Location;
            }
            catch (Exception ex) { }

            try
            {
                DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "Locallow") + @"\Art+Craft\Crowfall\CombatLogs\");
                FileInfo[] fi = di.GetFiles("*.log").OrderByDescending(p => p.CreationTime).ToArray();

                foreach (FileInfo f in fi)
                {
                    try
                    {
                        f.MoveTo(f.FullName.Replace(".log", ".txt"));
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex) { }

            try
            {
                foreach (Panel p in myPanels)
                {
                    p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);
                }

            }
            catch (Exception ex) { }

            try
            {
                string[] sets = null;

                try
                {
                    sets = Properties.Settings.Default["SettingsButtonSet"].ToString().Split(';');
                }
                catch (Exception ex) { }

                if (sets != null)
                {
                    try
                    {
                        if (!sets[0].Equals("0") || !sets[1].Equals("0"))
                        {
                            button2.Left = Convert.ToInt32(sets[0]);
                            button2.Top = Convert.ToInt32(sets[1]);
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        if (!sets[2].Equals("0") || !sets[3].Equals("0"))
                        {
                            button2.Width = Convert.ToInt32(sets[2]);
                            button2.Height = Convert.ToInt32(sets[3]);
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex) { }

            try
            {
                string[] sets = null;

                try
                {
                    sets = Properties.Settings.Default["SettingsButtonSetArchiv"].ToString().Split(';');
                }
                catch (Exception ex) { }

                if (sets != null)
                {
                    try
                    {
                        if (!sets[0].Equals("0") || !sets[1].Equals("0"))
                        {
                            btnArchiv.Left = Convert.ToInt32(sets[0]);
                            btnArchiv.Top = Convert.ToInt32(sets[1]);
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        if (!sets[2].Equals("0") || !sets[3].Equals("0"))
                        {
                            btnArchiv.Width = Convert.ToInt32(sets[2]);
                            btnArchiv.Height = Convert.ToInt32(sets[3]);
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex) { }

            foreach (Panel p in myPanels)
            {
                string[] sets = null;

                try
                {
                    sets = Properties.Settings.Default[p.Name + "Set"].ToString().Split(';');
                }
                catch (Exception ex) { }

                if (sets != null)
                {
                    try
                    {
                        if (!sets[0].Equals("0") || !sets[1].Equals("0"))
                        {
                            p.Left = Convert.ToInt32(sets[0]);
                            p.Top = Convert.ToInt32(sets[1]);
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        if (!sets[2].Equals("0") || !sets[3].Equals("0"))
                        {
                            p.Width = Convert.ToInt32(sets[2]);
                            p.Height = Convert.ToInt32(sets[3]);
                        }
                    }
                    catch (Exception ex) { }
                }

            }

            _myaccount = Account_.Text;
            _mygroup = ApiKey_.SelectedIndex.ToString();
            _myclass = cbClass.SelectedIndex.ToString();

            bgw_wait4file.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            panel1.Visible = false;

            if (Properties.Settings.Default.tut)
            {
                tme_live_view.Enabled = false;
                panel26.BringToFront();
                panel26.Dock = DockStyle.Fill;
                panel26.Visible = true;

                pnlttut.Left = Width / 2 - (pnlttut.Width / 2);
                pnlttut.Top = Height / 2 - (pnlttut.Height / 2);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex > -1)
            {

                string pers = listBox2.Items[listBox2.SelectedIndex].ToString().Split('(')[0].Trim();

                dgFights.DataSource = null;
                dgFights.AutoGenerateColumns = false;
                SQLiteDataAdapter da = new SQLiteDataAdapter("select min(dateTime) as date, max(dateTime), cID as name from stats where target = '" + pers + "' or caster = '" + pers + "' group by cID order by dateTime desc", m_dbConnection_readonly);
                DataTable fightID = new DataTable();
                da.Fill(fightID);
                fightID.Columns.Add("sec");

                foreach (DataRow dr in fightID.Rows)
                {
                    DateTime start = DateTime.Parse(dr[0].ToString());
                    DateTime stop = DateTime.Parse(dr[1].ToString());

                    var diffInSeconds = (stop - start).TotalSeconds;
                    if (diffInSeconds > 5)
                    {
                        TimeSpan elapsed = TimeSpan.FromSeconds(diffInSeconds);
                        string elapsedFormatted = elapsed.ToString(@"m\:ss");

                        dr[3] = elapsedFormatted;
                    }
                    else
                    {
                        dr.Delete();
                    }
                }

                dgFights.DataSource = fightID;

            }
        }

        private void checkBox16_Click(object sender, EventArgs e)
        {
            if (checkBox16.Checked)
            {
                OpenFileDialog fd = new OpenFileDialog();
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "Locallow") + @"\Art+Craft\Crowfall\CombatLogs\";
                fd.Filter = "|*.exe";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    if (fd.FileName.Contains("CrowfallLauncher.exe"))
                    {
                        Properties.Settings.Default.CrowfallFolder = fd.FileName;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void dgFights_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cID = dgFights.SelectedRows[0].Cells["col_cid"].Value.ToString();
                lastAction = DateTime.Now;
            }
            catch (Exception ex) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel26.Visible = false;

            tme_live_view.Enabled = true;
            Properties.Settings.Default.tut = false;
            Properties.Settings.Default.Save();

            try
            {
                foreach (Panel p in myPanels)
                {
                    p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);
                }

            }
            catch (Exception ex) { }


            foreach (Panel p in myPanels)
            {
                string[] sets = null;

                try
                {
                    sets = Properties.Settings.Default[p.Name + "Set"].ToString().Split(';');
                }
                catch (Exception ex) { }

                if (sets != null)
                {
                    try
                    {
                        if (!sets[0].Equals("0") || !sets[1].Equals("0"))
                        {
                            p.Left = Convert.ToInt32(sets[0]);
                            p.Top = Convert.ToInt32(sets[1]);
                        }
                    }
                    catch (Exception ex) { }

                    try
                    {
                        if (!sets[2].Equals("0") || !sets[3].Equals("0"))
                        {
                            p.Width = Convert.ToInt32(sets[2]);
                            p.Height = Convert.ToInt32(sets[3]);
                        }
                    }
                    catch (Exception ex) { }
                }

            }
        }

        private void btntutnext_Click(object sender, EventArgs e)
        {
            List<string> pages = new System.Collections.Generic.List<string>();

            pages.Add("Details window; ...");
            pages.Add("Archive; ...");
            pages.Add("DPS out Graph; ...");
            pages.Add("DPS in / HPS in Graph; ...");
            pages.Add("HPS out Graph; ...");
            pages.Add("Settingswindow; ...");
            pages.Add("DPS out skill Graph; ...");
            pages.Add("Raid details; ...");
            pages.Add("Raid Archive; ...");

            button3.Visible = false;
            if (tutpage < myPanels.Count)
            {
                if (myPanels[tutpage].Name.Equals("SettingsWindow"))
                {
                    tutpage++;
                }
                myPanels[tutpage].Parent = tutobj;
                myPanels[tutpage].Dock = DockStyle.Fill;
                myPanels[tutpage].Visible = true;

                tutHeadline.Text = pages[tutpage].Split(';')[0];
                tutText.Text = pages[tutpage].Split(';')[1];

                try
                {
                    myPanels[tutpage - 1].Dock = DockStyle.None;
                    myPanels[tutpage - 1].Parent = this;
                }
                catch (Exception ex) { }
            }
            else
            {
                tutHeadline.Text = "finished";
                tutText.Text = "have fun using this application, settings window will open now";
                button3.Visible = true;
                btntutnext.Visible = false;
            }
            tutpage++;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            tme_live_view.Enabled = false;
            panel26.BringToFront();
            panel26.Dock = DockStyle.Fill;
            panel26.Visible = true;

            pnlttut.Left = Width / 2 - (pnlttut.Width / 2);
            pnlttut.Top = Height / 2 - (pnlttut.Height / 2);
        }

        private void dgDetails_Leave(object sender, EventArgs e)
        {
            tme_live_view.Enabled = true;
        }

        private void pnltooltip_Leave(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            pnltooltip.Visible = false;
            tme_live_view.Enabled = true;
        }

        private void dgDetails_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                toolHeader.Text = "";
                tooltext.Text = "";

                for (int i = 1; i < assets.skills.Count - 1; i++)
                {
                    string power = dgDetails.SelectedRows[0].Cells[0].Value.ToString().Replace("_", " ").ToLower();
                    if (assets.skills[i].Split('|')[1].ToLower().Replace("'", "").Equals(power))
                    {
                        toolHeader.Text = assets.skills[i].Split('|')[1];
                        tooltext.Text = assets.skills[i].Split('|')[2] + "\r\n" + assets.skills[i].Split('|')[3] + "\r\n" + assets.skills[i].Split('|')[0];
                        pnltooltip.Visible = true;
                        pnltooltip.BringToFront();
                        break;
                    }
                }

                tme_live_view.Enabled = false;
            }
            catch (Exception ex) { }
            dgDetails.ClearSelection();
        }

        private void dgDetails_MouseLeave(object sender, EventArgs e)
        {
            tme_live_view.Enabled = true;
        }

        private void cbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            _myclass = cbClass.SelectedIndex.ToString();
            Properties.Settings.Default.Class_ = cbClass.SelectedIndex;
            Properties.Settings.Default.Save();
            thatclass = cbClass.Items[cbClass.SelectedIndex].ToString();

            lblClass.Text = cbClass.Items[cbClass.SelectedIndex].ToString();
            imgClass.Image = (Image)Properties.Resources.ResourceManager.GetObject("_" + cbClass.SelectedIndex.ToString());
            btnJoinRaid.PerformClick();
            btnJoinRaid.PerformClick();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "|*.csv";
            fd.FileName = DateTime.Now.ToFileTime().ToString() + ".csv";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string csv = "";
                tme_live_view.Enabled = false;
                SQLiteDataAdapter da1 = new SQLiteDataAdapter(
        "select * from stats where cID = '" + cID + "'", m_dbConnection_readonly);
                DataTable fightID1 = new DataTable();
                da1.Fill(fightID1);

                foreach (DataColumn dc in fightID1.Columns)
                {
                    csv += dc.ColumnName + ";";
                }
                csv += "\r\n";

                foreach (DataRow dr in fightID1.Rows)
                {
                    foreach (object s in dr.ItemArray)
                    {
                        csv += s.ToString() + ";";
                    }
                    csv += "\r\n";
                }

                tme_live_view.Enabled = true;
                File.WriteAllText(fd.FileName, csv);
            }

        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SettingsWindow.Visible = false;
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isMoving = true;
                movX = e.X;
                movY = e.Y;
            }
        }

        private void button2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving && !checkBox13.Checked)
            {
                ((PictureBox)sender).Left = MousePosition.X - movX;
                ((PictureBox)sender).Top = MousePosition.Y - movY;
            }
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {

            string[] oldSet = Properties.Settings.Default["SettingsButtonSet"].ToString().Split(';');
            oldSet[0] = ((PictureBox)sender).Left.ToString();
            oldSet[1] = ((PictureBox)sender).Top.ToString();
            string newSet = "";
            foreach (string s in oldSet)
            {
                newSet += s + ";";
            }
            Properties.Settings.Default["SettingsButtonSet"] = newSet;
            Properties.Settings.Default.Save();

            isMoving = false;
        }

        private void uiLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.uilang = uiLang.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void gameLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.gameLang = gameLang.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "|*.csv";
            fd.FileName = DateTime.Now.ToFileTime().ToString() + ".csv";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                string csv = "";
                tme_live_view.Enabled = false;
                SQLiteDataAdapter da1 = new SQLiteDataAdapter(
        "select * from stats where cID = '" + cID + "'", m_dbConnection_readonly);
                DataTable fightID1 = new DataTable();
                da1.Fill(fightID1);

                foreach (DataColumn dc in fightID1.Columns)
                {
                    csv += dc.ColumnName + ";";
                }
                csv += "\r\n";

                foreach (DataRow dr in fightID1.Rows)
                {
                    foreach (object s in dr.ItemArray)
                    {
                        csv += s.ToString() + ";";
                    }
                    csv += "\r\n";
                }

                tme_live_view.Enabled = true;
                File.WriteAllText(fd.FileName, csv);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (checkBox9.Checked == false)
            {
                foreach (Panel p in myPanels)
                {
                    if (p.Bounds.IntersectsWith(new Rectangle(MousePosition, new Size(10, 10))) && checkBox13.Checked == false)
                    {
                        foreach (Object o in p.Controls)
                        {
                            if (o.GetType() == typeof(Panel))
                            {
                                if (((Panel)o).Tag != null && ((Panel)o).Tag.Equals("1"))
                                {
                                    ((Panel)o).Show();
                                }
                            }

                        }
                    }
                    else
                    {
                        foreach (Object o in p.Controls)
                        {
                            if (o.GetType() == typeof(Panel))
                            {
                                if (((Panel)o).Tag != null && ((Panel)o).Tag.Equals("1"))
                                {
                                    ((Panel)o).Hide();
                                }
                            }

                        }

                    }
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            panel37.Visible = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            panel37.Visible = true;
            panel37.BringToFront();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            fromuser = Account_.Text;
            Fights.Hide();

            Properties.Settings.Default.Fights = Fights.Visible;
            Properties.Settings.Default.Save();
        }

        private void btnArchiv_Click(object sender, EventArgs e)
        {
            fromuser = Account_.Text;
            Fights.Visible = !Fights.Visible;

            Properties.Settings.Default.Fights = Fights.Visible;
            Properties.Settings.Default.Save();

            foreach (Panel p in myPanels)
            {
                if (!p.Name.Equals("SettingsWindow"))
                {
                    p.Visible = Convert.ToBoolean(Properties.Settings.Default[p.Name]);
                }
            }

            allhidden = false;
        }

        private void network_q_Tick(object sender, EventArgs e)
        {
            if (btnJoinRaid.Text.Equals("leave"))
            {
                if (NetworkQueue.Count > 0 && btnJoinRaid.Text.Equals("leave"))
                {
                    string data = "";
                    while (NetworkQueue.Count > 0)
                    {
                        data += NetworkQueue[0] + "|";
                        NetworkQueue.RemoveAt(0);
                    }

                    if (!bgPost.IsBusy)
                    {
                        bgPost.RunWorkerAsync(data);
                    }
                }
                else {
                }


                if (!bgGet.IsBusy)
                {
                    bgGet.RunWorkerAsync();
                }
            }
        }

        public void delete()
        {
            var request = WebRequest.CreateHttp("https://" + Properties.Settings.Default + ".firebaseio.com/.json");
            request.Method = "DELETE";
            request.ContentType = "application/json";
            var response = request.GetResponse();
        }

        public class netline{
            public string d { get; set; }
            public string t { get; set; }
        }

        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private void btnArchiv_MouseUp(object sender, MouseEventArgs e)
        {

            string[] oldSet = Properties.Settings.Default["SettingsButtonSetArchiv"].ToString().Split(';');
            oldSet[0] = ((PictureBox)sender).Left.ToString();
            oldSet[1] = ((PictureBox)sender).Top.ToString();
            string newSet = "";
            foreach (string s in oldSet)
            {
                newSet += s + ";";
            }
            Properties.Settings.Default["SettingsButtonSetArchiv"] = newSet;
            Properties.Settings.Default.Save();

            isMoving = false;
        }

        private void lstInGroup_Click(object sender, EventArgs e)
        {
            try
            {
                fromuser = lstInGroup.SelectedValue.ToString();
                lastAction = DateTime.Now;
            }
            catch (Exception ex) { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (btnJoinRaid.Text.Equals("leave")) {
                btnJoinRaid.PerformClick();
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (!btnJoinRaid.Text.Equals("join"))
            {
                btnJoinRaid_Click(sender, e);
            }
            else {
                RaidDPSout.Visible = false;
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            checkBox12.Checked = !checkBox12.Checked;

            Properties.Settings.Default.RaidDPSout = checkBox12.Checked;
            Properties.Settings.Default.Save();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "https://community.crowfall.com/topic/25682-deutsch-nm-nordic-marauders/";
            p.Start();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.ServerUrl = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            
            if (firstnetwork && sqlQueue.Count > 0) {
                try {
                    while (sqlQueue.Count > 0)
                    {
                        sqlQueue.RemoveAt(0);
                    }
                } catch (Exception ex) { }
            }

            timer4.Enabled = false;
            while (sqlQueue.Count > 0)
            {
                if (sqlQueue[0].Length > 5)
                {
                    try
                    {
                        DateTime curD = DateTime.Parse(sqlQueue[0].Split(',')[18].Replace("'", ""));
                        var diffInSeconds = (curD - lastD).TotalSeconds;

                        int FDuraI = Properties.Settings.Default.FightDura_;
                        if (btnJoinRaid.Text.Equals("leave")) {
                            FDuraI = 25;
                        }

                        if ((diffInSeconds > FDuraI  || cID == ""))  // || diffInSeconds < -3
                        {
                            cID = generateID();
                            cID_selected = cID;
                        }

                        if (!cID.Equals(cID_selected) && cID_selected.Length > 5)
                        {
                            cID = cID_selected;
                        }
                        lastD = curD;
                    }
                    catch (Exception ex) { }
                    string mySQL = sqlQueue[0].Replace("@@CID@@", cID);
                    mySQL = mySQL.Replace("@@", cID);

                    try
                    {
                        m_dbConnection_live.Open();
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        SQLiteCommand cmd = new SQLiteCommand(m_dbConnection_live);
                        cmd.CommandText = mySQL;
                        cmd.ExecuteNonQuery();
                        m_dbConnection_live.Close();
                        sqlQueue.RemoveAt(0);

                        lastAction = DateTime.Now;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    sqlQueue.RemoveAt(0);
                }
            }
            timer4.Enabled = true;
        }

        private void label48_Click(object sender, EventArgs e)
        {
            delete();
        }

        private void dgDPSOut_SelectionChanged(object sender, EventArgs e)
        {
            dgDPSOut.ClearSelection();
        }

        private void mergeFights_Click(object sender, EventArgs e)
        {
            if (dgFights.SelectedRows.Count > 1) {
                string firstCid = "";
                foreach (DataGridViewRow dr in dgFights.SelectedRows) {
                    if (firstCid.Length < 1) {
                        firstCid = dr.Cells["col_cid"].Value.ToString();
                    }
                    sqlQueue.Add("update stats set cID = '" + firstCid + "' where cID = '" + dr.Cells["col_cid"].Value.ToString() + "'");
                }
            }
        }

        public DateTime UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
            return dtDateTime;
        }

        public long DateTimeToUnix(DateTime MyDateTime)
        {
            TimeSpan timeSpan = MyDateTime - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)timeSpan.TotalSeconds;
        }

        private void bgPost_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            var content = new StringContent("{ \"d\":\"" + e.Argument + "\", \"t\":{\".sv\":\"timestamp\"}}", Encoding.UTF8, "application/json");
            CliOn.DefaultRequestHeaders.Clear();
            CliOn.DefaultRequestHeaders.ConnectionClose = false;
            var response = CliOn.PostAsync("https://" + Properties.Settings.Default.ServerUrl + ".firebaseio.com/.json", content).GetAwaiter().GetResult();
        }

        private void bgGet_DoWork(object sender, DoWorkEventArgs e)
        {
            string URL = "";
            if (lastLastInsertUnix == "" || ab == "0")
            {
                URL = "https://" + Properties.Settings.Default.ServerUrl + ".firebaseio.com/.json?limitToLast=1&orderBy=\"t\"";
                firstnetwork = true;
            }
            else {
                firstnetwork = false;
                
                try
                {
                    if (ab == (Convert.ToInt64(lastLastInsertUnix) - 2).ToString())
                    {
                        ab = (Convert.ToInt64(lastLastInsertUnix) - 1).ToString();
                    }
                    if (ab == (Convert.ToInt64(lastLastInsertUnix) - 1).ToString())
                    {
                        ab = (Convert.ToInt64(lastLastInsertUnix)).ToString();
                    }
                    else if (ab == lastLastInsertUnix)
                    {
                     //   ab = (Convert.ToInt64(lastLastInsertUnix) + 1).ToString();
                    }
                    else if (ab == (Convert.ToInt64(lastLastInsertUnix) + 1).ToString())
                    {

                    }
                    else
                    {
                        ab = (Convert.ToInt64(lastLastInsertUnix) - 2).ToString();
                    }
                }
                catch (Exception ex)
                {
                }
                URL = "https://" + Properties.Settings.Default.ServerUrl + ".firebaseio.com/.json?orderBy=\"t\"&startAt=" + ab; // " + (Convert.ToDouble(lastAction.ToString("yyyyMMddHHmmss")) - 20).ToString("#") +  "

            }

            string responseBody = "";
            CliOn.DefaultRequestHeaders.Clear();
            CliOn.DefaultRequestHeaders.ConnectionClose = false;
            var response = CliOn.GetAsync(URL).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                responseBody = responseContent.ReadAsStringAsync().GetAwaiter().GetResult();

            }
            
            var firebaseLookup = JsonConvert.DeserializeObject<Dictionary<string, netline>>(responseBody);
            var array = firebaseLookup.Values.ToList(); // or FirstOrDefault();

            if (array != null)
            {
                foreach (var content in array)
                {
                    if (NetQue.Count > 20) {
                        NetQue.RemoveAt(0);
                    }
                    if (NetQue.IndexOf(content.d.ToString()) == -1)
                    {
                        try
                        {
                            NetQue.Add(content.d.ToString());

                            string[] lines = content.d.ToString().Split('|');
                            foreach (string s in lines)
                            {

                                if (s.Length > 10)
                                {
                                    string values = s;
                                    string sqlval = "";
                                    int i = 0;
                                    foreach (string sca in values.Split(';'))
                                    {
                                        i++;
                                        string add = "";
                                        if (sca.Equals("A") && i > 2)
                                        {
                                            add = "True";
                                        }
                                        else if (sca.Equals("B") && i > 2)
                                        {
                                            add = "False";
                                        }
                                        else if (sca.Contains(':'))
                                        {
                                            add = UnixTimeToDateTime(Convert.ToInt64(content.t)).ToString("dd.MM.yyyy HH:mm:ss");
                                            lastLastInsertUnix = content.t;
                                        }
                                        else
                                        {
                                            add = sca;
                                        }
                                        sqlval += "'" + add + "',";
                                    }

                                    values = sqlval.Substring(0, sqlval.Length - 1);
                                    string sql = "insert into stats (fromuser ,charclass ,caster, skill, cID, dateTime, self, dmg, heal, restore, crit, target, action, amount)values(" + values + ")";

                                    sqlQueue.Add(sql);
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }

                }
            }
        }

        private void bgPost_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!bgGet.IsBusy) {
                bgGet.RunWorkerAsync();
            }
        }
       
        private void bgGet_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            cID = e.UserState.ToString();
        }
    }

}
