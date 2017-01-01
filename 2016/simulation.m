function simulation()
arrivalDistribution = [
	1, .125;
	2, .125;
	3, .125;
	4, .125;
	5, .125;
	6, .125;
	7, .125;
	8, .125
];
serviceDistribution = [
	1, .10;
	2, .20;
	3, .30;
	4, .25;
	5, .10;
	6, .05
];

n = 100000000;

arrivals = randsample(arrivalDistribution(:, 1)', n, true, arrivalDistribution(:, 2)');
services = randsample(serviceDistribution(:, 1)', n, true, serviceDistribution(:, 2)');

fprintf('N: %d\n', n);
%%%%%%%%%%%%%%%%% QUEUE SIMULATION %%%%%%%%%%%%%%%%%
waitingTimeAvg = main(arrivals, services, n);
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

arrivalMean = mean(arrivals);
arrivalVar = var(arrivals) ^ .5;
servicesMean = mean(services);
servicesVar = var(services) ^ .5;

lambda = 1 / arrivalMean;
tau = servicesMean;
ro = lambda / (1 / tau);
ca = arrivalVar / arrivalMean;
cs = servicesVar / servicesMean;

fprintf('Expected: %f, Actual: %f\n', ...
  ... % Kingman's formula (estimation)
  (ro / (1 - ro)) * (((ca ^ 2) + (cs ^ 2)) / 2) * tau, ...
  waitingTimeAvg);

end
function waitingTimeAvg = main(arrivals, services, n)
arrivals(1) = 0;

%init result
previousArrivalDiffArray(n) = .0;
arrivalTimeArray(n) = .0;
serviceDurationArray(n) = .0;
waitingTimeArray(n) = .0;
noCustomerTimeArray(n) = .0;

reservedQueue = 0;
arrivalTime = 0;

for k = 1:n
    currentEnter = arrivals(k);
    serviceDuration = services(k);

    noCustomerTime = 0;
    if reservedQueue < currentEnter
    noCustomerTime = currentEnter - reservedQueue;
    reservedQueue = 0;
    else
        reservedQueue = reservedQueue - currentEnter;
    end

    arrivalTime = arrivalTime + currentEnter;

    previousArrivalDiffArray(k) = currentEnter;
    arrivalTimeArray(k) = arrivalTime;
    serviceDurationArray(k) = serviceDuration;
    waitingTimeArray(k) = reservedQueue;
    noCustomerTimeArray(k) = noCustomerTime;

    reservedQueue = reservedQueue + serviceDuration;
end

serviceStartArray = arrivalTimeArray + waitingTimeArray;
serviceEndArray = serviceStartArray + serviceDurationArray;
customerInSystemTimeArray = waitingTimeArray + serviceDurationArray;

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

%{
waitingTimeSum = sum(waitingTimeArray);
waitedCustomers = sum(waitingTimeArray(:) ~= 0);
noCustomerTimeSum = sum(noCustomerTimeArray);
serviceDurationSum = sum(serviceDurationArray);
previousArrivalDiffSum = sum(previousArrivalDiffArray);
customerInSystemTimeSum = sum(customerInSystemTimeArray);

fprintf('avg waitingTime: %f\n', waitingTimeSum / n);
fprintf('waitedCustomers / n: %f\n', waitedCustomers / n);
fprintf('noCustomerTimeSum / lastCustomer.serviceEnd: %f\n', ...
    noCustomerTimeSum / serviceEndArray(n));
fprintf('avg serviceDuration: %f\n', serviceDurationSum / n);
fprintf('avg previousArrivalDiff): %f\n', ...
    previousArrivalDiffSum / (n - 1));
fprintf('waitingTimeSum / waitedCustomers: %f\n', ...
    waitingTimeSum / waitedCustomers);
fprintf('avg customerInSystemTime: %f\n', customerInSystemTimeSum / n);
fprintf('avg noCustomerTime: %f\n', noCustomerTimeSum / n);
%}
waitingTimeAvg = mean(waitingTimeArray);
end