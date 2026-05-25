// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-book-carousel]").forEach((carousel) => {
    const track = carousel.querySelector("[data-carousel-track]");
    const viewport = carousel.querySelector(".genre-section__viewport");
    const previousButton = carousel.querySelector("[data-carousel-prev]");
    const nextButton = carousel.querySelector("[data-carousel-next]");

    if (!track || !viewport || !previousButton || !nextButton) {
      return;
    }

    let currentIndex = 0;

    const getStep = () => {
      const firstCard = track.querySelector(".book-card");
      if (!firstCard) {
        return 0;
      }

      const styles = window.getComputedStyle(track);
      const gap = parseFloat(styles.columnGap || styles.gap) || 0;
      return firstCard.getBoundingClientRect().width + gap;
    };

    const getMaxIndex = () => {
      const step = getStep();
      if (step === 0) {
        return 0;
      }

      const hiddenWidth = track.scrollWidth - viewport.clientWidth;
      return Math.max(0, Math.ceil(hiddenWidth / step));
    };

    const updateCarousel = () => {
      const step = getStep();
      const maxIndex = getMaxIndex();
      currentIndex = Math.min(currentIndex, maxIndex);

      track.style.transform = `translateX(${-currentIndex * step}px)`;
      previousButton.disabled = currentIndex === 0;
      nextButton.disabled = currentIndex >= maxIndex;
    };

    previousButton.addEventListener("click", () => {
      currentIndex = Math.max(0, currentIndex - 1);
      updateCarousel();
    });

    nextButton.addEventListener("click", () => {
      currentIndex = Math.min(getMaxIndex(), currentIndex + 1);
      updateCarousel();
    });

    window.addEventListener("resize", updateCarousel);
    updateCarousel();
  });
});
