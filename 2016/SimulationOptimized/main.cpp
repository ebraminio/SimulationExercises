#include <cstdlib>
#include <cstdio>
#include <ctime>
#include <cmath>
#include <ctime>
#include <map>
#include <vector>
#include <iostream>
#include <cinttypes>
#include <fstream>

#include <thread>

class ItemPicker {
  uint8_t* items;
  int* aggregatedPossbilities;
  unsigned long n;

public:

  ItemPicker(std::map<uint8_t, float> possibilities) {
    n = possibilities.size();

    items = new uint8_t[n];
    aggregatedPossbilities = new int[n];

    float aggreagation = 0;
    int i = 0;
    for (auto const item : possibilities) {
      aggreagation += item.second;
      aggregatedPossbilities[i] = aggreagation * RAND_MAX;
      items[i] = item.first;
      i++;
    }
  }

  ~ItemPicker() {
    delete items;
    delete aggregatedPossbilities;
  }

  inline uint8_t pick(int value) {
    for (int i = 0; i < n; ++i) {
      if (value <= aggregatedPossbilities[i])
        return items[i];
    }
  }
};

typedef struct {
  //int id;
  uint64_t previousArrivalDiff;
  uint64_t arrivalTime;
  uint64_t serviceDuration;
  uint64_t serviceStart;
  uint64_t waitingTime;
  uint64_t serviceEnd;
  uint64_t customerInSystemTime;
  uint64_t noCustomerTime;
} RestaurantCustomer;

class Restaurant {
  ItemPicker* enteringDifferenceStream;
  ItemPicker* serviceTimeStream;

public:

  Restaurant() {
    enteringDifferenceStream = new ItemPicker({
        { 1, .125 },
        { 2, .125 },
        { 3, .125 },
        { 4, .125 },
        { 5, .125 },
        { 6, .125 },
        { 7, .125 },
        { 8, .125 }
    });
    serviceTimeStream = new ItemPicker({
        { 1, .10 },
        { 2, .20 },
        { 3, .30 },
        { 4, .25 },
        { 5, .10 },
        { 6, .05 }
    });
  }

  ~Restaurant() {
    delete enteringDifferenceStream;
    delete serviceTimeStream;
  }

  inline RestaurantCustomer generate(bool first, uint64_t arrivalTime, uint64_t reservedQueue,
    int currentEnterSeed, int currentServiceTimeSeed) {
    uint64_t currentEnter = first ? 0 : enteringDifferenceStream->pick(currentEnterSeed);
    first = false;
    uint64_t currentServiceTime = serviceTimeStream->pick(currentServiceTimeSeed);

    uint64_t noCustomerTime = 0;
    if (reservedQueue < currentEnter) {
      noCustomerTime = currentEnter - reservedQueue;
      reservedQueue = 0;
    }
    else {
      reservedQueue -= currentEnter;
    }

    arrivalTime += currentEnter;

    return{
      //++customerId,
      currentEnter,
      arrivalTime,
      currentServiceTime,
      arrivalTime + reservedQueue,
      reservedQueue,
      arrivalTime + reservedQueue + currentServiceTime,
      reservedQueue + currentServiceTime,
      noCustomerTime
    };
  };
};

void test();
void testBN();

uint64_t i = 0;
uint64_t n = 1000000;
uint64_t waitingTimeSum = 0;
uint64_t waitedCustomers = 0;
uint64_t noCustomerTimeSum = 0;
uint64_t serviceDurationSum = 0;
uint64_t previousArrivalDiffSum = 0;
uint64_t customerInSystemTimeSum = 0;
uint64_t arrivalTime = 0;
uint64_t reservedQueue = 0;
bool finished = false;
RestaurantCustomer previousCustomer;

clock_t begin;

void printStats() {
  printf("\nResult of the simulation with %lld cases:", i);
  printf("\nElapsed time: %fs", double(std::clock() - begin) / CLOCKS_PER_SEC);
  printf("\nAverage waiting time: %f", (double)waitingTimeSum / n);
  printf("\nRatio of waited customers: %f", (double)waitedCustomers / n);
  printf("\nRatio of no customer times: %f", (double)noCustomerTimeSum / previousCustomer.serviceEnd);
  printf("\nService duration average: %f", (double)serviceDurationSum / n);
  printf("\nAverage of customers entering leaps: %f", (double)previousArrivalDiffSum / (n - 1));
  printf("\nAverage of waited customers waiting times: %f",
    (double)waitingTimeSum / (waitedCustomers == 0 ? 1 : waitedCustomers));
  printf("\nAverage of customers being in system time: %f\n", (double)customerInSystemTimeSum / n);
}

void printStatsTask() {
  while (getchar() && !finished) printStats();
}

int g_seed;
// http://stackoverflow.com/a/3747462/1414809
inline int fastrand() {
  g_seed = (214013 * g_seed + 2531011);
  return (g_seed >> 16) & 0x7FFF;
}

int main() {
  // init random generator
  g_seed = std::time(0);

  Restaurant restaurant;

  printf("Press space to see the simulation state...\n");
  std::thread printStatsThread(printStatsTask);
  begin = std::clock();

  bool first = true;
  for (i = 0; i < n; ++i) {
    auto customer = restaurant.generate(first, arrivalTime, reservedQueue, fastrand(), fastrand());

    first = false;
    waitingTimeSum += customer.waitingTime;
    if (customer.waitingTime != 0) ++waitedCustomers;
    noCustomerTimeSum += customer.noCustomerTime;
    serviceDurationSum += customer.serviceDuration;
    previousArrivalDiffSum += customer.previousArrivalDiff;
    customerInSystemTimeSum += customer.customerInSystemTime;

    previousCustomer = customer;
    arrivalTime = customer.arrivalTime;
    reservedQueue = customer.customerInSystemTime;
  }
  finished = true;
  printStats();

  test();

  printStatsThread.join();

  return 0;
}

void test() {
  Restaurant restaurant;

  int n = 20;
  float enterDiffRandomNumbers[] = {
          0, .913, .727, .015, .948, .309, .922, .753, .235, .302,
          .109, .093, .607, .738, .359, .888, .106, .212, .393, .535
  };
  float serviceTime[] = {
          .84, .10, .74, .53, .17, .79, .91, .67, .89, .38,
          .32, .94, .79, .05, /*.79*/.94, .84, .52, .55, .30, .50
  };

  std::vector<RestaurantCustomer> expected = {
          { /*1,*/ 0, 0, 4, 0, 0, 4, 4, 0 },
          { /*2,*/ 8, 8, 1, 8, 0, 9, 1, 4 },
          { /*3,*/ 6, 14, 4, 14, 0, 18, 4, 5 },
          { /*4,*/ 1, 15, 3, 18, 3, 21, 6, 0 },
          { /*5,*/ 8, 23, 2, 23, 0, 25, 2, 2 },
          { /*6,*/ 3, 26, 4, 26, 0, 30, 4, 1 },
          { /*7,*/ 8, 34, 5, 34, 0, 39, 5, 4 },
          { /*8,*/ 7, 41, 4, 41, 0, 45, 4, 2 },
          { /*9,*/ 2, 43, 5, 45, 2, 50, 7, 0 },
          { /*10,*/ 3, 46, 3, 50, 4, 53, 7, 0 },
          { /*11,*/ 1, 47, 3, 53, 6, 56, 9, 0 },
          { /*12,*/ 1, 48, 5, 56, 8, 61, 13, 0 },
          { /*13,*/ 5, 53, 4, 61, 8, 65, 12, 0 },
          { /*14,*/ 6, 59, 1, 65, 6, 66, 7, 0 },
          { /*15,*/ 3, 62, 5, 66, 4, 71, 9, 0 },
          { /*16,*/ 8, 70, 4, 71, 1, 75, 5, 0 },
          { /*17,*/ 1, 71, 3, 75, 4, 78, 7, 0 },
          { /*18,*/ 2, 73, 3, 78, 5, 81, 8, 0 },
          { /*19,*/ 4, 77, 2, 81, 4, 83, 6, 0 },
          { /*20,*/ 5, 82, 3, 83, 1, 86, 4, 0 }
  };

  printf("\n");

  uint64_t arrivalTime = 0;
  uint64_t reservedQueue = 0;
  bool first = true;

  for (int i = 0; i < n; ++i) {
    auto result = restaurant.generate(first, arrivalTime, reservedQueue,
      enterDiffRandomNumbers[i] * RAND_MAX, serviceTime[i] * RAND_MAX);
    first = false;
    bool actualMatchesExpected =
      /*expected[i].id == result.id &&*/ expected[i].arrivalTime == result.arrivalTime &&
      expected[i].previousArrivalDiff == result.previousArrivalDiff &&
      expected[i].serviceDuration == result.serviceDuration &&
      expected[i].serviceStart == result.serviceStart && expected[i].waitingTime == result.waitingTime &&
      expected[i].serviceEnd == result.serviceEnd &&
      expected[i].customerInSystemTime == result.customerInSystemTime &&
      expected[i].noCustomerTime == result.noCustomerTime;
    printf(actualMatchesExpected ? "PASSED! " : "FAIL! ");

    arrivalTime = result.arrivalTime;
    reservedQueue = result.customerInSystemTime;
  }
}
