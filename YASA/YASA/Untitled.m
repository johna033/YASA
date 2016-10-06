func = [1 2; 3 4];
[FX FY]=gradient(func);

func1 = [exp(sin(1)) exp(sin(2))];
gradient(func1, sin(2)-sin(1));
exp(sin(2))