// Dimtiar Atanassov

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppP2
{
    //A delegate is a contract between publisher(Crusie) and subscriber(ticketAgent)
    public delegate void priceCutEventHandler(Int32 price, String AgentName); //When the price of a ticket is cut, and we want to notify the subscribers of the price change we should have a method with these parameters and return type.
    public delegate void newOrderEvent();   //Event to trigger every time there is a new order
    public delegate void orderCheckedEvent(OrderClass orderObject, double totalPrice);  //Event is triggered every time the order is processed/checked
    public class Cruise
    {
        public static event priceCutEventHandler priceCut;
        //Creating a ticket price
        static Random rand = new Random();  //Random Object that is used to generate a random number
        public static Int32 ticketPrice = rand.Next(40, 201); // Generates a Random number between 40 and 200 (Inital Ticket Price)
        public static Int32 prevTicketPrice = 800;
        private static Int32 counter = 0;    //Keeps track of the number of price cuts that have occured
        public static Int32 agentCount = 0;

        public void PricingModel()
        {
            //After counter = 20 price cuts have been made cruise thread will terminate
            while(counter < 20)
            {
                Thread.Sleep(2000);
                //This can be rand number can be greater or less than the current price
                Int32 newRandTicketPrice = rand.Next(40, 201);  //This newRandTicketPrice is given to ticketPrice inside of onPriceCut function
                onPriceCut(newRandTicketPrice);
                counter++;
            }
            Program.openThread = false; //Price cut counter has exceeded limit so thread is no longer needed

        }

        //Function to get the price of a ticket 
        public Int32 getPrice()
        {
            //Price of ticket for the cruise
            return ticketPrice;
        }
        public Int32 getPrevTicketPrice()
        {
            return prevTicketPrice;
        }

        //This method notifys the subscribers if there is a price cut and updates the ticketPrice
        public static void onPriceCut(Int32 price)
        {
            //This checks if there are any subcribers to our event
            if (priceCut != null)
            {
                //Checks if price is reduced
                if (price < ticketPrice)
                {
                    if(agentCount < Program.ticketAgents.Length)
                    {
                        agentCount = 0;
                    }
                    priceCut(ticketPrice, Program.ticketAgents[agentCount].Name);  //Emitting the event to our subscribers (same signature as our delegate)
                    agentCount++;
                }
                if (price != ticketPrice && price < ticketPrice)    //Need to keep track of prevTicket price to determine order quantity
                {
                    prevTicketPrice = ticketPrice;
                    ticketPrice = price;    //Update ticket price
                }
            }
        }

        // Event handler for a new order
        public void newOrderHandler()
        {
            OrderClass newOrder = Program.buffer.getOneCell();  //Getting the new order that was created from our ticket agent and stored in a multithreadbuffer
            //Creating a orderProcess thread for the new order created by the ticket agent
            Thread orderProcessThread = new Thread(() => OrderProcesser.processOrder(newOrder));
            orderProcessThread.Start();
        }

    }

    public class OrderProcesser
    {
        private static double tax = 0.5;
        private static double locationCharge = 100;
        public static event orderCheckedEvent orderChecked;
        public static void processOrder(OrderClass newOrder)
        {
            Monitor.Enter(newOrder);
            //Checking if payment is valid
            if (validateCreditCardNum(newOrder.getCardNumber()) == true)
            {
                double totalChargeAmount = newOrder.getUnitPrice() * newOrder.getQuantity() + tax + locationCharge;
                Console.WriteLine("Created Order for: Agent {0}", newOrder.getThreadID());
                orderChecked(newOrder, totalChargeAmount); 

            }
            else
            {
                Console.WriteLine("Agent {1} Card Num: {0} is not valid", newOrder.getCardNumber(), newOrder.getThreadID());
            }
            Monitor.Exit(newOrder);
        }
        public static Boolean validateCreditCardNum(Int32 cardNum)
        {
            if (cardNum > 5000 && cardNum <= 7000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class TicketAgent
    {

        public static Int32 prevTicketPrice;
        public static Int32 newTicketPrice;
        public static Int32 baseQuantity;
        public static Cruise cruise = new Cruise();
        public static event newOrderEvent newOrderAlert; //Event to signal that there is a new order
        public static Random rand = new Random();

        public void ticketAgentFunc()   //For starting thread
        {
            while(Program.openThread == true)
            {
                Thread.Sleep(2000);
                createTicketAgentOrder(Thread.CurrentThread.Name);
            }
        }
        //Creating a new order for the ticket agent and storing it in a buffer
        public void createTicketAgentOrder(string AgentName)
        {
            OrderClass orderObj = new OrderClass();
            orderObj.setThreadID(AgentName);
            Int32 newCardNumber;
            baseQuantity = quantityCalc();
            newCardNumber = rand.Next(4080,7010);
            orderObj.setCardNumber(newCardNumber);
            orderObj.setQuantity(baseQuantity);
            orderObj.setUnitPrice(newTicketPrice);
            Program.buffer.setOneCell(orderObj);
            newOrderAlert();
        }

        //Event handler for priceCut event
        public void ticketOnSale(Int32 price, string AgentName)
        {
            prevTicketPrice = cruise.getPrevTicketPrice();
            newTicketPrice = price;
            createTicketAgentOrder(AgentName);
        }
        //Helper function to calculate amount of tickets to buy
        public Int32 quantityCalc()
        {
            baseQuantity = 10;
            baseQuantity = 10 * (prevTicketPrice - newTicketPrice);
            return baseQuantity;
        }
        //Display message if the order is processed
        public void orderProccesed(OrderClass newOrder, double totalChargeAmount)
        {
            if(newOrder.getQuantity() > 0)
            {
                 Console.WriteLine("Agent {0}'s Order: {1} Tickets for a total of {2}, each priced at {3} (Paid with Card: {4})", newOrder.getThreadID(), newOrder.getQuantity(), totalChargeAmount, newOrder.getUnitPrice(), newOrder.getCardNumber());
                 Console.WriteLine();
            }
        }
    }


    public class OrderClass
    {
        private string threadId;  //Identity of sender
        private Int32 cardNumber;    //int to represent a credit card number
        private Int32 quantity; //int to represent the number of tickets to order
        private double unitPrice; // a double that represents the unit price of the tricket recieved from the cruise

        //Getters and Setters

        public string getThreadID() { return threadId; }
        public Int32 getCardNumber() { return cardNumber; }
        public Int32 getQuantity() { return quantity; }
        public double getUnitPrice() { return unitPrice; }

        public void setThreadID(string newID) { this.threadId = newID; }
        public void setCardNumber(Int32 newCardNum) { this.cardNumber = newCardNum; }
        public void setQuantity(Int32 newQuantity) { this.quantity = newQuantity; }
        public void setUnitPrice(Double newUnitPrice) { this.unitPrice = newUnitPrice; }
    }

    public class MultiCellBuffer
    {
        private OrderClass[] cells;
        private static Semaphore open;
        private static Semaphore full;
        int cellIndex = 0;
        private object lockObject = new Object();
        //Constructor for MultiCellBuffer
        public MultiCellBuffer()
        {
            //Each cell is a refrence to an orderclass object
            cells = new OrderClass[2];
            cells[0] = null;
            cells[1] = null;
            open = new Semaphore(2, 2);
            full = new Semaphore(0, 2);
        }
        //Methods to write and read data from one of the open cells
        //Write
        public void setOneCell(OrderClass newOrder)
        {
            open.WaitOne();
            lock (lockObject)
            {
        
                cells[cellIndex] = newOrder;
                cellIndex++;
              
            }
            full.Release();
        }
        //read
        public OrderClass getOneCell()
        {
            OrderClass orderObj = new OrderClass();
            full.WaitOne();
            lock (lockObject)
            {
                cellIndex--;
                orderObj = cells[cellIndex];
            }
            open.Release();
            return orderObj;
        }
    }
    public class Program
    {
        public static MultiCellBuffer buffer = new MultiCellBuffer();
        public static Thread[] ticketAgents;
        public static Boolean openThread = true;
        static void Main(string[] args)
        {
            //Creating Objects for threads
            TicketAgent agent = new TicketAgent();
            Cruise cruiseMain = new Cruise();


            Thread calculateTicketPrice = new Thread(new ThreadStart(cruiseMain.PricingModel)); // Thread for Cruise
            calculateTicketPrice.Start();

            Cruise.priceCut += new priceCutEventHandler(agent.ticketOnSale);    //Notifying subscribers of event there has been a price cut
            TicketAgent.newOrderAlert += new newOrderEvent(cruiseMain.newOrderHandler); ////Notifying subscribers of event there has been a new ordered created
            OrderProcesser.orderChecked += new orderCheckedEvent(agent.orderProccesed); ////Notifying subscribers of event there has been a new order processed
            ticketAgents = new Thread[5];   
            for(int n = 0; n < 5; n++)
            {
                ticketAgents[n] = new Thread(new ThreadStart(agent.ticketAgentFunc));
                ticketAgents[n].Name = n.ToString();
                ticketAgents[n].Start();
            }
        }
    }
}
