document.addEventListener("DOMContentLoaded", () => {
    const stage = document.getElementById("mmBullStage");
    const bull = document.getElementById("mmBullImage");

    if (!stage || !bull) {
        return;
    }

    let targetX = 0;
    let targetY = 0;
    let currentX = 0;
    let currentY = 0;

    const animateBull = () => {
        currentX += (targetX - currentX) * 0.12;
        currentY += (targetY - currentY) * 0.12;

        bull.style.transform =
            `translateX(4%) translateY(2%) rotateX(${currentY}deg) rotateY(${currentX}deg) scale(1.01)`;

        requestAnimationFrame(animateBull);
    };

    stage.addEventListener("mousemove", (event) => {
        const bounds = stage.getBoundingClientRect();

        const mouseX = (event.clientX - bounds.left) / bounds.width;
        const mouseY = (event.clientY - bounds.top) / bounds.height;

        targetX = (mouseX - 0.5) * 10;
        targetY = (mouseY - 0.5) * -7;
    });

    stage.addEventListener("mouseleave", () => {
        targetX = 0;
        targetY = 0;
    });

    animateBull();
});



document.addEventListener("DOMContentLoaded", function () {
    const successAlert =
        document.getElementById("contactSuccessAlert");

    if (!successAlert) {
        return;
    }

    const contactSection =
        document.getElementById("contact");

    contactSection?.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });

    setTimeout(function () {
        successAlert.style.transition =
            "opacity 0.35s ease, transform 0.35s ease";

        successAlert.style.opacity = "0";
        successAlert.style.transform = "translateY(-8px)";

        setTimeout(function () {
            successAlert.remove();
        }, 350);

    }, 3500);
});