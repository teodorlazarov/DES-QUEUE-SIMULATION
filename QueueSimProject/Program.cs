using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTake
{
    class Program
    {
        public static double clock,                // Часовник на моделното време
                           meanInterArrivalTime, // Средно време между пристигането
                           meanServiceTime,      // Средна продължителност на обслужването
                           meanServiceTimeSecond,      // Средна продължителност на обслужването
                           lastEventTime,        // Момент на последнотосъбитие
                           totalBusy,            // Обща продължителност на заетостта на ОУ
                           totalBusySecond,            // Обща продължителност на заетостта на ОУ
                           maxQueueLength,       // Максимална дължина на опашката
                           maxQueueLengthSecond,       // Максимална дължина на опашката
                           sumResponseTime,
                           secondQueueArrivalDifference,
                           secondLastArrived;
        public static long queueLength,          // Дължина на заявката
                                numberInService,
                                numberInServiceSecond,
                                totalCustomers,       // Общ брой заявки
                                numberOfDepartures,   // Брой на обслужените заявки
                                longService,
                                numberOfRejectedCustomers = 0,
                                secondQueueLength;
        public const int ARRIVAL = 1;          // Тип на събитието "Постъпване на заявка в ОУ1"
        public const int SEND_TO_SECOND = 2;        // Тип на събитието "Постъпване на заявка в ОУ2"
        public const int DEPARTURE = 3;        // Тип на събитието "Край на обслужването"
        public static List<Event> futureEventList;      // Списък на предстоящите събития
        public static Queue<Event> customers;                // Опашка от заявки към ОУ1
        public static Queue<Event> customersSecond;                // Опашка от заявки към ОУ2
        public static Random stream;                  // Генератор на псевдослучайни числа

        private const int TOTAL_CUSTOMERS = 1000;

        public static void Main(string[] args)
        {
            meanInterArrivalTime = 2.5;            // Среден интервал между постъпващите заявки λ
            meanServiceTime = 2.7;                 // Средна продължителност на времето за обслужване в ОУ1 μ1
            meanServiceTimeSecond = 3.0;           // Средна продължителност на времето за обслужване в ОУ2 μ2
            totalCustomers = TOTAL_CUSTOMERS;      // Максимален брой заявки в модела
            long seed = 2376457;                   // Стойност за инициализация на генератора
                                                   
            stream = new Random((int)seed);          // Инициализация на генератора за случайни числа
            futureEventList = new List<Event>();     // Инициализация на списъка от предстоящ събития
            customers = new Queue<Event>();          // Инициализация на опашката към ОУ1
            customersSecond = new Queue<Event>();    // Инициализация на опашката към ОУ2

            Init();     // Инициализация на симулацията

            while (numberOfDepartures < totalCustomers)
            {
                if (futureEventList.Count <= 0)  //Ако в списъкът със събития няма събития, да се добави ново събития от тип "Постъпване в ОУ1"
                {
                    double eventTime = clock + Exponential(stream, meanInterArrivalTime);
                    Event nextArrival = new Event(ARRIVAL, eventTime);
                    futureEventList.Add(nextArrival);
                }
                Event currentEvent = futureEventList.GetMin();
                futureEventList.Remove(currentEvent);
                clock = currentEvent.Time;
                //Ако събитието е от тип "Постъпване в ОУ1" и броя на хората в опашката е по-малък от 20,
                //то била обработено
                if (currentEvent.Type == ARRIVAL && customers.Count < 20)
                {
                    ProccessArrival(currentEvent);
                }
                //Ако събитието е от ти "Постъпване в ОУ2" и има заявки в опашката, то се обработва
                else if (currentEvent.Type == SEND_TO_SECOND)
                {
                    if (customers.Count > 0)
                    {
                        ProccessDeparture(currentEvent);
                    }
                }
                //Ако събитието е от ти "Край на обслужването" и има заявки в опашката, то се обработва
                else if (currentEvent.Type == DEPARTURE)
                {
                    if (customersSecond.Count > 0)
                    {
                        ProccessDepartureSecond(currentEvent);
                    }
                }
                //Отхвърля се събитието, ако е тип "Постъпване в ОУ1 и ако в първата опашка има повече от 20 заявки 
                else if (currentEvent.Type == ARRIVAL && customers.Count >= 20)
                {
                    numberOfRejectedCustomers++;
                }
            }
            ReportGeneration();                     // Извеждане на информация за моделирания процес
            Console.ReadLine();
        }

        //Обработка на събитие тип "Постъпване в ОУ1"
        private static void ProccessArrival(Event currentEvent)
        {
            customers.Enqueue(currentEvent);
            queueLength++;
            //Console.WriteLine(queueLength);

            if (numberInService == 0)
            {
                ScheduleDeparture();
            }
            else
            {
                totalBusy += (clock - lastEventTime); // общо време на заетост на ОУ
            }
            // Следене на максималната дължина на опашката
            if (maxQueueLength < queueLength)
            {
                maxQueueLength = queueLength;
            }
            // Планиране на момента на следващата заявка     
            double eventTime = clock + Exponential(stream, meanInterArrivalTime);
            Event nextArrival = new Event(ARRIVAL, eventTime);
            futureEventList.Add(nextArrival);
            lastEventTime = clock;
        }

        //Обработва напускането на заявката от ОУ1 и изпраща събитието към ОУ2
        private static void ProccessDeparture(Event currentEvent)
        {
            ProccessArrivalAtSecond(currentEvent);
            Event finished = customers.Dequeue();
            // Ако има заявки в опашката се планира момента на завършване на обслужването на следващата
            if (queueLength > 0)
            {
                ScheduleDeparture();
            }
            else
            {
                numberInService = 0;
            }

            //Събираме данни за изчисляване на средния интервал на постъпване на заявки в опашка 2 (λ2)
            secondQueueLength++;
            secondQueueArrivalDifference += currentEvent.Time - secondLastArrived;
            secondLastArrived = currentEvent.Time;

            // Измерва времето за реакция
            double response = clock - finished.Time;
            //Console.WriteLine(response);
            sumResponseTime += response;
            if (response > 4.0)
                longService++; // record long service
            totalBusy += (clock - lastEventTime);
            lastEventTime = clock;
        }

        //Генерира събитие от тип "Постъпване в ОУ2"
        private static void ScheduleDeparture()
        {
            double serviceTime = Exponential(stream, meanServiceTime);
            Event depart = new Event(SEND_TO_SECOND, clock + serviceTime);
            futureEventList.Add(depart);
            numberInService = 1;
            queueLength--;
        }

        //Обработка на събитие тип "Постъпване в ОУ2"
        private static void ProccessArrivalAtSecond(Event currentEvent)
        {
            customersSecond.Enqueue(currentEvent);

            if (maxQueueLengthSecond < customersSecond.Count())
            {
                maxQueueLengthSecond = customersSecond.Count();
            }

            if (numberInServiceSecond == 0)
            {
                ScheduleDepartureSecond();
            }
            else
            {
                totalBusySecond += (clock - lastEventTime); // общо време на заетост на ОУ
            }
        }

        //Генерира събитие от тип "Край на обслужването"
        private static void ScheduleDepartureSecond()
        {
            double serviceTime = Exponential(stream, meanServiceTimeSecond);
            Event depart = new Event(DEPARTURE, clock + serviceTime);
            futureEventList.Add(depart);
            numberInServiceSecond = 1;
        }

        //Обработка на събитието "Край на облужването" и обработва данни за статистика
        private static void ProccessDepartureSecond(Event currentEvent)
        {

            Event finished = customersSecond.Dequeue();
            // Ако има заявки в опашката се планира момента на завършване на обслужването на следващата
            if (customersSecond.Count > 0)
            {
                ScheduleDepartureSecond();
            }
            else
            {
                numberInServiceSecond = 0;
            }
            // Measure the response time and update cumulative statistics
            double response = clock - finished.Time;
            sumResponseTime += response;
            if (response > 4.0)
                longService++; // record long service
            totalBusySecond += (clock - lastEventTime);
            numberOfDepartures++;
            lastEventTime = clock;
        }

        //Инициализиране на данните за симулацията
        private static void Init()
        {
            clock = 0.0;                             
            queueLength = 0;
            numberInService = 0;
            lastEventTime = 0.0;
            totalBusy = 0;
            maxQueueLength = 0;
            sumResponseTime = 0;
            numberOfDepartures = 0;
            longService = 0;
            // Събитие "първо постъпване на заявка"
            double eventTime = Exponential(stream, meanInterArrivalTime);
            Event firstevent = new Event(ARRIVAL, eventTime);
            futureEventList.Add(firstevent);
        }

        public static double Exponential(Random rand, double parm)
        {
            return -(Math.Log(rand.NextDouble()) / parm);
        }

        private static void ReportGeneration()
        {
            double rho = totalBusy / clock;      // Коефициент на заетост на системата
            double rho2 = totalBusySecond / clock;
            double avgr = sumResponseTime / totalCustomers;   // 
            double pc4 = ((double)longService) / totalCustomers;
            double ro1 = meanInterArrivalTime / meanServiceTime;
            //Средна дължина на опашка 1
            double Lq1 = Math.Pow(ro1, 2) / (1 - ro1);
            //Средно време за чакане в опашка 1
            double W1 = Lq1 / meanInterArrivalTime;

            double meanSecondInterArrivalTime = secondQueueLength / secondQueueArrivalDifference;
            double ro2 = meanSecondInterArrivalTime / meanServiceTimeSecond;
            //Средна дължина на опашка 2
            double Lq2 = Math.Pow(ro2, 2) / (1 - ro2);
            //Средно време за чакане в опашка 2
            double W2 = Lq2 / meanSecondInterArrivalTime;

            Console.WriteLine("SINGLE SERVER QUEUE SIMULATION - GROCERY STORE CHECKOUT COUNTER ");
            Console.WriteLine("\tMEAN INTERARRIVAL TIME       " + meanInterArrivalTime);
            Console.WriteLine("\tMEAN SERVICE TIME            " + meanServiceTime);
            Console.WriteLine("\tMEAN SERVICE TIME 2           " + meanServiceTimeSecond);
            Console.WriteLine("\tNUMBER OF CUSTOMERS SERVED   " + totalCustomers);
            Console.WriteLine();
            Console.WriteLine("\tSERVER UTILIZATION           " + rho);
            Console.WriteLine("\tSERVER UTILIZATION 2           " + rho2);
            Console.WriteLine("\tMAXIMUM LINE LENGTH          " + maxQueueLength);
            Console.WriteLine("\tMAXIMUM LINE LENGTH 2          " + maxQueueLengthSecond);
            Console.WriteLine("\tAVERAGE RESPONSE TIME        " + avgr + " Time Units");
            Console.WriteLine("\tPROPORTION WHO SPEND FOUR ");
            Console.WriteLine("\t MINUTES OR MORE IN SYSTEM   " + pc4);
            Console.WriteLine("\tSIMULATION RUNLENGTH         " + clock + " Time Units");
            Console.WriteLine("\tNUMBER OF DEPARTURES         " + totalCustomers);
            Console.WriteLine("\tNUMBER OF REJECTED CUSTOMERS         " + numberOfRejectedCustomers);
            Console.WriteLine("\tMEAN WAIT TIME IN QUEUE 1        " + W1);
            Console.WriteLine("\tMEAN QUEUE 1 LENGTH          " + Lq1);
    
            Console.WriteLine("\tMEAN WAIT TIME IN QUEUE 2        " + W2);
            Console.WriteLine("\tMEAN QUEUE 2 LENGTH       " + Lq2);
        }
    }
}
