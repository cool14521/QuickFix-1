using QuickFix;
using QuickFix.FIX42;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Message = QuickFix.Message;
using System.Threading.Tasks;
using QuickFix.Fields;
using QuickFix.Transport;

namespace QuickFixManager.Data
{
    public class QuickFixService : MessageCracker, IApplication
    {
        private Session _session;

        public enum QuickFixType { Initiator, Acceptor }

        public QuickFixService(string configFile, QuickFixType type)
        {
            switch(type)
            {
                case QuickFixType.Acceptor: initAcceptor(configFile); break;
                case QuickFixType.Initiator: initInitiator(configFile); break;
                default: throw new NotSupportedException();
            }
        }

        public QuickFixService(string file)
        {
            initAcceptor(file);
            initInitiator(file);
        }

        public void FromAdmin(Message message, SessionID sessionId)
        {
            Console.WriteLine("From");
        }

        public void initAcceptor(string file)
        {
            SessionSettings settings = new SessionSettings(file);
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(
                this,
                storeFactory,
                settings,
                logFactory
            );

            acceptor.Start();
            var i = 0;
            while (true)
            {
                Console.WriteLine("listening for {0} seconds...", i);
                System.Threading.Thread.Sleep(1000);
                ++i;
            }
            acceptor.Stop();
        }

        public void initInitiator(string file)
        {
            SessionSettings settings = new SessionSettings(file);
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);

            SocketInitiator initiator = new SocketInitiator(
                this,
                storeFactory,
                settings,
                logFactory
            );

            initiator.Start();
            Run();
            initiator.Stop();
        }

        public void FromApp(Message message, SessionID sessionId)
        {
            Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void OnCreate(SessionID sessionId)
        {
            _session = Session.LookupSession(sessionId);
            Console.WriteLine("SenderCompID: {0}, TargetCompID: {1}, BeginString: {2}",
                sessionId.SenderCompID,
                sessionId.TargetCompID,
                sessionId.BeginString
            );
        }

        public void OnLogon(SessionID sessionId)
        {
            Console.WriteLine("Successful Login");
        }

        public void OnLogout(SessionID sessionId)
        {
            Console.WriteLine("Successful Logout");
        }

        public void ToAdmin(Message message, SessionID sessionId)
        {
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetField(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }

        public void OnMessage(QuickFix.FIX42.NewOrderSingle order, SessionID sessionId)
        {
            Console.WriteLine("ClOrdID: {0}", order.ClOrdID);
            Console.WriteLine("HandlInst: {0}", order.HandlInst);
            Console.WriteLine("Symbol: {0}", order.Symbol);
            Console.WriteLine("Side: {0}", order.Side);
            Console.WriteLine("TransactTime: {0}", order.TransactTime);
            Console.WriteLine("OrdType: {0}", order.OrdType);

            Console.WriteLine("Amount: {0}", order.OrderQty);
            //Console.WriteLine("Duration: {0}", order.); // Duration???
            Console.WriteLine("Price: {0}", order.Price);
            //Console.WriteLine("Limit Price: {0}", order.StopPx);
            //Console.WriteLine("Notes: {0}", order.); // Notes???
        }

        public void OnMessage(QuickFix.FIX42.SecurityDefinition secDef, SessionID sessionId)
        {
            Console.WriteLine(secDef);
        }

        public void Run()
        {
            QuickFix.FIX42.NewOrderSingle order = NewOrderSingle();
            order.Header.GetField(Tags.BeginString);
            SendMessage(order);
        }

        public QuickFix.FIX42.NewOrderSingle NewOrderSingle()
        {
            var order = new QuickFix.FIX42.NewOrderSingle(
                new ClOrdID("1234"),
                new HandlInst('1'),
                new Symbol("ZVZZT"),
                new Side(Side.BUY),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.MARKET)
            );

            order.Price = new Price(new decimal(25.05));
            order.Account = new Account("123456789");
            order.OrderQty = new OrderQty(new decimal(500));
            //order.StopPx = new StopPx(new decimal(28.00));

            return order; 
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }
    }
}
