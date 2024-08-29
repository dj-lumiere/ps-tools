#pragma once
#include <algorithm>
#include <utility>
#include <type_traits>

#if defined(__GNUC__) || defined(__clang__)
// print __int128_t with std::cout's operator<<
inline std::ostream& operator<<(std::ostream& os, const __int128_t& target)
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

// print __uint128_t with std::cout's operator<<
inline std::ostream& operator<<(std::ostream& os, const __uint128_t& target)
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
#endif

// Print pair
template<typename T, typename U>
std::ostream& operator<<(std::ostream& os, const std::pair<T, U>& target)
{
    os << "(" << target.first << ", " << target.second << ")";
    return os;
}

// Print tuple
template<typename Tuple, std::size_t N>
struct TuplePrinter
{
    static void print(std::ostream& os, const Tuple& t)
    {
        TuplePrinter<Tuple, N - 1>::print(os, t);
        os << ", " << std::get<N - 1>(t);
    }
};

template<typename Tuple>
struct TuplePrinter<Tuple, 1>
{
    static void print(std::ostream& os, const Tuple& t)
    {
        os << std::get<0>(t);
    }
};

template<typename... Elements>
std::ostream& operator<<(std::ostream& os, const std::tuple<Elements...>& t)
{
    os << "(";
    TuplePrinter<decltype(t), sizeof...(Elements)>::print(os, t);
    os << ")";
    return os;
}

// Base template for iterable containers (std::vector, std::list, etc.)
template<typename Iterable, typename = void>
struct is_iterable : std::false_type
{
};

template<typename Iterable>
struct is_iterable<Iterable, std::void_t<decltype(std::begin(std::declval<Iterable>())), decltype(std::end(std::declval<Iterable>()))> > : std::true_type{};

// Generalized operator<< overload for iterable containers
template<typename Iterable, typename std::enable_if<not std::is_same<Iterable, std::string>::value and is_iterable<Iterable>::value, int>::type = 1>
std::ostream& operator<<(std::ostream& os, const Iterable& iterable)
{
    os << "[";
    for (auto it = std::begin(iterable); it != std::end(iterable); ++it)
    {
        os << *it;
        if (std::next(it) != std::end(iterable))
        {
            os << ", ";
        }
    }
    os << "]";
    return os;
}

// Overload for C-style arrays excluding C-style strings
template<typename element, size_t size, typename std::enable_if<not std::is_same<element, char>::value and not std::is_same<element, wchar_t>::value, int>::type = 0>
std::ostream& operator<<(std::ostream& os, const element (& target)[size])
{
    os << "[";
    size_t i = 0;
    for (const element& e : target)
    {
        os << e;
        if (i + 1 != size)
        {
            os << ", ";
        }
        i += 1;
    }
    os << "]";
    return os;
}
