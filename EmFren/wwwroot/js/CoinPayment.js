// ============================================================
            // OPEN MODAL
            // ============================================================

            function openModal(modalId) {

                const modal =
                    document.getElementById(modalId);

                modal.classList.add("show");
            }


            // ============================================================
            // CLOSE MODAL
            // ============================================================

            function closeModal(modalId) {

                const modal =
                    document.getElementById(modalId);

                modal.classList.remove("show");
            }


            // ============================================================
            // VIEW MODAL
            // ============================================================

            function openViewModal(id, name) {

                document.getElementById("viewCoinId").value = id;

                document.getElementById("viewCoinName").value = name;

                openModal("viewModal");
            }


            // ============================================================
            // EDIT MODAL
            // ============================================================

            function openEditModal(id, name) {

                document.getElementById("editCoinId").value = id;

                document.getElementById("editCoinIdDisplay").value = id;

                document.getElementById("editCoinName").value = name;

                openModal("editModal");
            }


            // ============================================================
            // DELETE CONFIRMATION
            // ============================================================

            function confirmDelete(name) {

                return confirm(
                    "¿Está seguro de que desea eliminar la moneda \"" +
                    name +
                    "\"?"
                );
            }


            // ============================================================
            // CLOSE MODAL WHEN CLICKING OUTSIDE
            // ============================================================

            window.addEventListener("click", function(event) {

                if (event.target.classList.contains("modal")) {

                    event.target.classList.remove("show");

                }

            });


            // ============================================================
            // ESC KEY CLOSES MODAL
            // ============================================================

            document.addEventListener("keydown", function(event) {

                if (event.key === "Escape") {

                    document
                        .querySelectorAll(".modal.show")
                        .forEach(function(modal) {

                            modal.classList.remove("show");

                        });

                }

            });