# Первое занятие
## Начало (условно в 12:00)
1. Проходим [тест](https://forms.gle/fWuMLseguYdtJJHW8) + разбор ответов (10 мину тест + 20 минут разбор)   
Проверяем усвоение [материала на дом](https://rsdn.org/article/dotnet/CSThreading1.xml#E6B)
2. Краткое содержание домашнего материала (20 мин презентация)
   1. Мотивирующая часть (за чем это все)
   2. Процессы и потоки
   3. Планировщик потоков
3. Задача (20 минут сами + 10 минут подсказки + 10 минут эталон)
   1. Эксперементальным путем найти квант времени используя шаблон [QuantumOfSwitching](samples/QuantumOfSwitching/)
   2. Поиграться с настройками Windows (Adjust for best perfomance of programs / background services)
   3. Поиграться с [Concurrency Visualizer](https://docs.microsoft.com/lb-lu/visualstudio/profiling/concurrency-visualizer)    
    **`Thread.Yield`** vs **`Thread.Sleep(1)`** vs **`Thread.Sleep(0)`**
## Прошло 1:30
### Перерыв
10 минут отдыхаем
### Пул потоков
1. Рассказ про пул потоков (5 минут) - что это такое и зачем нужен
2. Задача: написать свой пул потоков (30 минут на выполнение + 10 минут рассказ про Monitor.Pulse/Wait + 15 минут реализация эталон).   
**Запретить BlockingQueue (ConcurrentStack, ConcurrentQueue, ConcurrentBag) и SpinWait.SpinUntil**
3. Рассказ про пул потоков в .NET (10 мин)
## Прошло 1:10
### Обед
Уходим на обед на 30-40 минут
### Продолжаем
1. Lock-free (общее описание + стек) (30 минут)
   1. Interlocked + задача написать LockFreeStack
   2. ConcurrentCollections (только рассказ)
2. Volatile (20 минут)
## Прошел 1 час
### Перерыв
Перерыв 10 минут
1. Формулировка домашнего задания про ReaderWriterLock (5 минут) (homework 1)
2. Если осталось время, то делаем
## Завершение примерно 17:40