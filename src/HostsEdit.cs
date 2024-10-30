using System.Text;
using System.Text.RegularExpressions;

namespace Host
{
    public class Edit
    {
        public string[] RegexStr(string RawStr)
        {
            string IpPattern=@"\b(([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])\b";//ip的正则表达式
            string DomainPattern=@"\b([a-zA-Z0-9]{1,63}://){0,1}([a-zA-Z0-9]{1,63}\.){1,2}([a-zA-Z0-9]{1,63})(\.[a-zA-Z0-9]{1,63}){0,1}\b";//域名的正则表达式

            Match IpMatch=Regex.Match(RawStr,IpPattern);//套用ip的正则表达式，抓取ip
            string IpStr=IpMatch.ToString();//将抓取到的ip转换成字符串类型

            
            string ModifyStr;
            if(IpStr.Length==0)
            {
                throw new ArgumentException("输入的内容未捕获到ip,追加失败...");
            }
            else
            {
                ModifyStr=RawStr.Replace(IpStr,"");//.ToString();//输入的原始数据截去ip,留存剩下的字符串
            }

            //string DomainPattern=@"[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+";
            
            Match DomainMatch=Regex.Match(ModifyStr,DomainPattern);//套用域名的正则表达式，抓取到域名
            string DomainStr=DomainMatch.ToString();

            if(DomainStr.Length==0)
            {
                throw new ArgumentException("输入的内容未捕获到域名,追加失败...");
            }
            else
            {
                string[] Ip_Domain=new string[]{IpStr,DomainStr};
                return Ip_Domain;
            }

        }

        public string[] Check(string Path)
        {
            if(File.Exists(Path))
            {
                File.SetAttributes(Path, FileAttributes.Normal);//更改文件属性为“正常”
                Console.WriteLine("发现hosts文件,已修改文件属性为正常...");
            }
            else
            {
                FileStream file=File.Create(Path);
                File.SetAttributes(Path,FileAttributes.Normal);
                file.Flush();
                file.Close();
                Console.WriteLine("未发现hosts文件,已新建并设置文件属性为正常...");
            }

            string RawData=File.ReadAllText(Path,Encoding.Default);//以文本形式打开一个文件，使用默认编码格式，读取所有字符
            string[] Stage_Data=new string[]{"Raw",RawData};
            return Stage_Data;
        }

        public void Query(string Stage,string Data)
        {
            if(Stage=="Raw")
            {
                Console.ForegroundColor=ConsoleColor.Green;
                Console.WriteLine("begin-------------------------原host文件内容-----------------------------");
                Console.WriteLine(Data);
                Console.WriteLine("------------------------------原host文件内容--------------------------end");
                Console.ForegroundColor=ConsoleColor.White;
            }
            else if(Stage=="Modify")
            {
                Console.ForegroundColor=ConsoleColor.Yellow;
                Console.WriteLine("begin------------------------修改后的host文件内容------------------------");
                Console.WriteLine(Data);
                Console.WriteLine("-----------------------------修改后的host文件内容---------------------end");
                Console.ForegroundColor=ConsoleColor.White;
                Console.WriteLine("hosts文件更新成功...");
            }
            else
            {
                Console.ForegroundColor=ConsoleColor.Red;
                Console.WriteLine("重复写入,追加失败...");
                Console.ForegroundColor=ConsoleColor.White;
            }
            return;
        }

        public string[] UpdateHosts(string Path,string RawData,string Ip,string Domain)
        {
            FileStream sf=new FileStream(Path,FileMode.Append);//FileMode.Append:打开文件，用于向文件中追加内容，如果文件不存在，则创建一个新文件。
            File.SetAttributes(Path,FileAttributes.Normal);//在“文件不存在，新建文件”的情况下，更改文件属性为“正常”
            StreamWriter sw=new StreamWriter(sf,Encoding.Default);//Encoding.Default:默认使用与操作系统一致的编码方式
            //避免重复写入
            if(RawData.Contains(Ip) && RawData.Contains(Domain))//<string>Contains(<str>),判断字符串中是否包含指定的字符或字符串
            {
                File.SetAttributes(Path,FileAttributes.ReadOnly);//设置文件属性为"只读"
                string[] Ip_Domain=new string[]{Ip,Domain};
                sw.Flush();//清空缓冲区
                sw!.Close();//关闭流
                sf.Close();
                return Ip_Domain;
            }
            else
            {
                //写入为追加模式
                string IpDomain=Ip+" "+Domain;
                sw.WriteLine(IpDomain);

                //关闭写入
                if(sw!=null)
                sw.Flush();//清空缓冲区
                sw!.Close();//关闭流
                sf.Close();

                string ModifyData=File.ReadAllText(Path,Encoding.Default);
                string[] Stage_Data=new String[]{"Modify",ModifyData};
                return Stage_Data;
            }
        } 
         public static void Main()
        {
            Console.Title="HostsEdit";
            string Path=@"C:\Windows\System32\drivers\etc\hosts";
            Edit Edit=new Edit();
            string[] RawData=Edit.Check(Path);
            Edit.Query(RawData[0],RawData[1]);
            
            while(true)
            {

                Console.Write("请输入ip地址和域名: ");
                string RawStr=Console.ReadLine()!;

                string[] Ip_Domain;
                try
                {
                    Ip_Domain=Edit.RegexStr(RawStr);
                }
                catch(ArgumentException a)
                {
                    Console.ForegroundColor=ConsoleColor.Red;
                    Console.WriteLine(a.Message);
                    Console.ForegroundColor=ConsoleColor.White;
                    continue;
                }

                string[] ModifyData=Edit.UpdateHosts(Path,RawData[1],Ip_Domain[0],Ip_Domain[1]);
                Edit.Query(ModifyData[0],ModifyData[1]);
                Console.WriteLine("");
                Console.WriteLine("(空格键)-->继续     or     (Ctrl键+C键)-->退出");
                Console.WriteLine("");
                Console.ReadKey();
            }
        }
    }
}