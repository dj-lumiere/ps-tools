#include <cstdint>
#include <iostream>
#include <vector>
#include <array>
#include <deque>
#include <list>
#include <map>
#include <set>
#include <unordered_map>
#include <unordered_set>
#include "forceprint.hpp"

int main()
{
	__int128_t a1 = 123456789;a1 *= 1e14;
	__uint128_t b1 = 987654321;b1 *= 1e14;
	std::vector<int32_t> a = { 1, 2, 3 };
	std::vector<std::vector<int32_t> > a2 = { { 1 }, { 2, 3 }, { 4, 5, 6 } };
	std::array<int64_t, 4> b = { 11111111111, 22222222222, 33333334333, 44444444444 };
	std::list<char> c = { 'a', 'b', 'c', 'd' };
	std::pair<char, char> d = { 'a', 'b' };
	std::tuple<char, char, char> e = { 'a', 'b', 'c' };
	std::map<char, char> f = { { 'a', 'b' }, { 'b', 'c' } };
	std::multimap<char, char> g = { { 'a', 'b' }, { 'b', 'c' } };
	std::unordered_map<char, char> h = { { 'a', 'b' }, { 'a', 'c' }, { 'b', 'c' } };
	std::unordered_multimap<char, char> i = { { 'a', 'b' }, { 'a', 'c' }, { 'b', 'c' } };
	std::set<char> j = { 'a', 'b' };
	std::multiset<char> k = { 'a', 'a', 'b', 'b' };
	std::unordered_set<char> l = { 'a', 'b' };
	std::unordered_multiset<char> m = { 'a', 'a', 'b', 'b' };
	std::deque<double> n = { 1.0, 2.0 };
	int o[3] = { 1, 2, 3 };
	int p[4][3] = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
	char q[4] = "abc";
	const char r[3][6] = { "home", "sweet", "home" };
	constexpr char s[4] = "abc";
	const char* t = "abc";
	std::string u = "Lumi made this";
	std::cout
		<< "__int128_t a1=" << a1 << "\n"
		<< "__uint128_t b1=" << b1 << "\n"
		<< "std::vector<int32_t> a=" << a << "\n"
		<< "std::vector<std::vector<int32_t>> a2=" << a2 << "\n"
		<< "std::array<int64_t, 4> b=" << b << "\n"
		<< "std::list<char> c=" << c << "\n"
		<< "std::pair<char, char> d=" << d << "\n"
		<< "std::tuple<char, char, char> e=" << e << "\n"
		<< "std::map<char, char> f=" << f << "\n"
		<< "std::multimap<char, char> g=" << g << "\n"
		<< "std::unordered_map<char, char> h=" << h << "\n"
		<< "std::unordered_multimap<char, char> i=" << i << "\n"
		<< "std::set<char> j=" << j << "\n"
		<< "std::multiset<char> k=" << k << "\n"
		<< "std::unordered_set<char> l=" << l << "\n"
		<< "std::unordered_multiset<char> m=" << m << "\n"
		<< "std::deque<double> n=" << n << "\n"
		<< "int o[3]=" << o << "\n"
		<< "int p[4][3]=" << p << "\n"
		<< "char q[4]=" << q << "\n"
		<< "const char r[3][6]=" << r << "\n"
		<< "constexpr char s[4]=" << s << "\n"
		<< "const char* t=" << t << "\n"
		<< "std::string u=" << u << "\n";
}
