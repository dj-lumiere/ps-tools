#ifndef forceprint_h
#define forceprint_h
#pragma once
#include <iostream>
#include <algorithm>
#include <vector>
#include <complex>
#include <random>
#include <unordered_map>
#include <unordered_set>
#include <deque>
#include <queue>
#include <cmath>
#include <list>
#include <map>
#include <set>
#include <cstdint>
#include <utility>

// Print pair
template<typename T, typename U> std::ostream& operator<<(std::ostream& os, const std::pair<T, U>& target)
{
	os << "(" << target.first << ", " << target.second << ")";
	return os;
}

// Print __int128_t
std::ostream& operator<<(std::ostream& os, const __int128_t& target)
{
	if (target == 0)
	{
		os << '0';
		return os;
	}
	__int128_t temp = target;
	if (temp < 0)
	{
		os << '-';
		temp = -temp;
	}
	std::string answer;
	while (temp > 0)
	{
		answer.push_back(static_cast<char>('0' + temp % 10));
		temp /= 10;
	}
	std::reverse(answer.begin(), answer.end());
	os << answer;
	return os;
}

// Print __uint128_t
std::ostream& operator<<(std::ostream& os, const __uint128_t& target)
{
	if (target == 0)
	{
		os << '0';
		return os;
	}
	__uint128_t temp = target;
	std::string answer;
	while (temp > 0)
	{
		answer.push_back(static_cast<char>('0' + temp % 10));
		temp /= 10;
	}
	std::reverse(answer.begin(), answer.end());
	os << answer;
	return os;
}

// Print vector
template<typename element> std::ostream& operator<<(std::ostream& os, const std::vector<element>& target)
{
	os << "[";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "]";
	return os;
}

// Print array
template<typename element, size_t size> std::ostream& operator<<(std::ostream& os, const std::array<element, size>& target)
{
	os << "[";
	for (uint64_t i = 0; i < size; i++)
	{
		os << target[i];
		if (i + 1 != size)
		{
			os << ", ";
		}
	}
	os << "]";
	return os;
}

// Print linked list
template<typename element> std::ostream& operator<<(std::ostream& os, const std::list<element>& target)
{
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << " <-> ";
		}
	}
	return os;
}

// Print unordered_set
template<typename element> std::ostream& operator<<(std::ostream& os, const std::unordered_set<element>& target)
{
	os << "{";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "}";
	return os;
}

// Print set
template<typename element> std::ostream& operator<<(std::ostream& os, const std::set<element>& target)
{
	os << "{";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "}";
	return os;
}

// Print multiset
template<typename element> std::ostream& operator<<(std::ostream& os, const std::multiset<element>& target)
{
	os << "{";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "}";
	return os;
}

// Print unordered_map
template<typename Key, typename Value> std::ostream& operator<<(std::ostream& os, const std::unordered_map<Key, Value>& target)
{
	os << "{";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << it->first << ": " << it->second;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "}";
	return os;
}

// Print map
template<typename Key, typename Value> std::ostream& operator<<(std::ostream& os, const std::map<Key, Value>& target)
{
	os << "{";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << it->first << ": " << it->second;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "}";
	return os;
}

// Print deque
template<typename element> std::ostream& operator<<(std::ostream& os, const std::deque<element>& target)
{
	os << ">";
	for (auto it = target.begin(); it != target.end(); ++it)
	{
		os << *it;
		if (std::next(it) != target.end())
		{
			os << ", ";
		}
	}
	os << "<";
	return os;
}

// Print tuple
template<typename... elements> std::ostream& operator<<(std::ostream& os, const std::tuple<elements...>& target)
{
	uint64_t length = std::tuple_size(target);
	os << "(";
	for (uint64_t i = 0; i < length; i++)
	{
		os << std::get<i>(target);
		if (i + 1 != length)
		{
			os << ", ";
		}
	}
	os << ")";
	return os;
}

#endif
