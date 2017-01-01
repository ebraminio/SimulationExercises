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

#include "picojson.h"
#include "num.hpp"

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
  Num previousArrivalDiff;
  Num arrivalTime;
  Num serviceDuration;
  Num serviceStart;
  Num waitingTime;
  Num serviceEnd;
  Num customerInSystemTime;
  Num noCustomerTime;
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

  inline RestaurantCustomer generate(bool first, Num arrivalTime, Num reservedQueue,
      int currentEnterSeed, int currentServiceTimeSeed) {
    Num currentEnter = first ? 0 : enteringDifferenceStream->pick(currentEnterSeed);
    first = false;
    Num currentServiceTime = serviceTimeStream->pick(currentServiceTimeSeed);

    Num noCustomerTime = 0;
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

  Num arrivalTime = 0;
  Num reservedQueue = 0;
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

clock_t begin;
Num i = 0;
Num n = 20000000;
Num waitingTimeSum = 0;
Num waitedCustomers = 0;
Num noCustomerTimeSum = 0;
Num serviceDurationSum = 0;
Num previousArrivalDiffSum = 0;
Num customerInSystemTimeSum = 0;
Num arrivalTime = 0;
Num reservedQueue = 0;
bool finished = false;
RestaurantCustomer previousCustomer;

uint64_t mantissaMultiplicant = 100000;
inline double bigNumDiv(Num num, Num divisor) {
  return (waitingTimeSum * mantissaMultiplicant / divisor).to_double() / mantissaMultiplicant;
}

std::string numToString(Num num) {
  std::vector<char> v;
  num.print(v);
  return std::string(v.data());
}

picojson::value numToJsonValue(Num num) {
  return picojson::value(numToString(num));
}

Num jsonValueToNum(picojson::value value) {
  Num num(value.to_str().c_str());
  return num;
}

void printStats() {
  printf("\nResult of the simulation with %s cases:", numToString(i).c_str());
  printf("\nElapsed time: %fs", double(std::clock() - begin) / CLOCKS_PER_SEC);
  printf("\nAverage waiting time: %f", bigNumDiv(waitingTimeSum, n));
  printf("\nRatio of waited customers: %f", bigNumDiv(waitedCustomers, n));
  printf("\nRatio of no customer times: %f", bigNumDiv(noCustomerTimeSum, previousCustomer.serviceEnd));
  printf("\nService duration average: %f", bigNumDiv(serviceDurationSum, n));
  printf("\nAverage of customers entering leaps: %f", bigNumDiv(previousArrivalDiffSum, (n - 1)));
  printf("\nAverage of waited customers waiting times: %f",
    bigNumDiv(waitingTimeSum.to_double(), (waitedCustomers == 0 ? 1 : waitedCustomers)));
  printf("\nAverage of customers being in system time: %f\n", bigNumDiv(customerInSystemTimeSum, n));
}

int g_seed;
// http://stackoverflow.com/a/3747462/1414809
inline int fastrand() {
  g_seed = (214013 * g_seed + 2531011);
  return (g_seed >> 16) & 0x7FFF;
}

void readState() {
  std::ifstream lastState("result.json");
  if (lastState.good()) {
    picojson::value json;
    lastState >> json;
    waitingTimeSum = jsonValueToNum(json.get("waitingTimeSum"));
    waitedCustomers = jsonValueToNum(json.get("waitedCustomers"));
    noCustomerTimeSum = jsonValueToNum(json.get("noCustomerTimeSum"));
    serviceDurationSum = jsonValueToNum(json.get("serviceDurationSum"));
    previousArrivalDiffSum = jsonValueToNum(json.get("previousArrivalDiffSum"));
    customerInSystemTimeSum = jsonValueToNum(json.get("customerInSystemTimeSum"));
    arrivalTime = jsonValueToNum(json.get("arrivalTime"));
    reservedQueue = jsonValueToNum(json.get("reservedQueue"));
    i += jsonValueToNum(json.get("i"));
  }
}

void saveState() {
  picojson::object json;
  json["waitingTimeSum"] = numToJsonValue(waitingTimeSum);
  json["waitedCustomers"] = numToJsonValue(waitedCustomers);
  json["noCustomerTimeSum"] = numToJsonValue(noCustomerTimeSum);
  json["serviceDurationSum"] = numToJsonValue(serviceDurationSum);
  json["previousArrivalDiffSum"] = numToJsonValue(previousArrivalDiffSum);
  json["customerInSystemTimeSum"] = numToJsonValue(customerInSystemTimeSum);
  json["arrivalTime"] = numToJsonValue(arrivalTime);
  json["reservedQueue"] = numToJsonValue(reservedQueue);
  json["i"] = numToJsonValue(i);

  std::ofstream result("result.json", std::ios::trunc);
  result << picojson::value(json);
  result.close();
}

void printStatsTask() {
  while (getchar() && !finished) {
    saveState();
    printStats();
  }
}

int main() {
  // init random generator
  g_seed = std::time(0);

  readState();
  Restaurant restaurant;

  printf("Press space to see the simulation state...\n");
  std::thread printStatsThread(printStatsTask);

  begin = std::clock();
  bool first = true;
  Num till = i + n;
  for (; i < till; ++i) {
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
  saveState();
  //testBN();
  test();

  printStatsThread.join();

  return 0;
}
