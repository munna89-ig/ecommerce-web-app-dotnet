// ================= HERO BANNER SLIDER =================
(function () {
    const hero = document.querySelector('.hero');
    if (!hero) return;

    const banners = hero.querySelectorAll('.banner');
    if (banners.length < 2) return;          // nothing to slide

    const DELAY = 3000;
    let index = 0;
    let timer = null;

    const pad = () => parseInt(getComputedStyle(hero).paddingLeft) || 0;

    function goTo(i) {
        hero.scrollTo({
            left: banners[i].offsetLeft - hero.offsetLeft - pad(),
            behavior: 'smooth'
        });
    }

    function next() {
        index = (index + 1) % banners.length;
        goTo(index);
    }

    function start() {
        stop();
        timer = setInterval(next, DELAY);
    }

    function stop() {
        clearInterval(timer);
    }

    hero.addEventListener('mouseenter', stop);
    hero.addEventListener('mouseleave', start);
    hero.addEventListener('touchstart', stop, { passive: true });
    hero.addEventListener('touchend', () => setTimeout(start, 2000), { passive: true });

    // keep "index" in sync when the user swipes
    hero.addEventListener('scroll', () => {
        let closest = 0;
        let minDist = Infinity;

        banners.forEach((b, i) => {
            const dist = Math.abs(b.offsetLeft - hero.offsetLeft - pad() - hero.scrollLeft);
            if (dist < minDist) {
                minDist = dist;
                closest = i;
            }
        });

        index = closest;
    }, { passive: true });

    document.addEventListener('visibilitychange', () => {
        document.hidden ? stop() : start();
    });

    start();
})();

// ================= NEW ARRIVALS SLIDER =================
(function () {
    const track = document.getElementById('naTrack');
    const prev = document.getElementById('naPrev');
    const next = document.getElementById('naNext');
    if (!track || !prev || !next) return;

    function step() {
        const gap = parseInt(getComputedStyle(track).columnGap) || 20;
        return track.clientWidth + gap;   // one page (4 cards) at a time
    }

    function updateButtons() {
        const max = track.scrollWidth - track.clientWidth;
        prev.classList.toggle('hidden', track.scrollLeft <= 5);
        next.classList.toggle('hidden', track.scrollLeft >= max - 5);
    }

    next.addEventListener('click', () => track.scrollBy({ left: step(), behavior: 'smooth' }));
    prev.addEventListener('click', () => track.scrollBy({ left: -step(), behavior: 'smooth' }));

    track.addEventListener('scroll', updateButtons, { passive: true });
    window.addEventListener('resize', updateButtons);
    updateButtons();
})();

// ================= ALL ITEMS "SEE MORE" =================
(function () {
    const btn = document.getElementById('seeMoreBtn');
    const grid = document.getElementById('itemsGrid');
    if (!btn || !grid) return;

    btn.addEventListener('click', function () {
        const isShowingAll = btn.classList.contains('active');

        grid.querySelectorAll('.extra-item').forEach(function (item) {
            item.classList.toggle('hidden', isShowingAll);
        });

        btn.textContent = isShowingAll ? 'See More' : 'See Less';
        btn.classList.toggle('active');
    });
})();