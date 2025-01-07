typedef uint32_t RansState;

constexpr uint32_t RANS_L = 1u << 16;

constexpr uint32_t k = 10; // для примера
constexpr uint32_t RANS_M = 1u << k; // M = 2^k

// Кодируем символ s
void RansEnc(RansState& x, uint32_t s, RansOutBuf& out)
{
    assert(x >= RANS_L); // Этот инвариант должен выполняться всегда

    uint32 Fs = freq[s]; // Частота символа s
    uint32 Bs = range_start[s]; // Начало интервала s

    assert(Fs > 0 && Fs <= RANS_M);

    // renormalize
    if ((x >> 16) >= (RANS_L >> k) * Fs) { // x / b >=  L / M * Fs

        out.put(x & 0xffff);

        x >>= 16;
    }

    x = ((x / Fs) << k) + Bs + (x % Fs); // C(s,x)

    assert(x >= RANS_L); // Этот инвариант должен выполняться всегда
}